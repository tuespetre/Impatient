using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Projection;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Generating;

public class QueryCompilingExpressionVisitor : ExpressionVisitor
{
    private static readonly MethodInfo asQueryableMethodInfo
        = ReflectionExtensions.GetGenericMethodDefinition((IEnumerable<object> e) => e.AsQueryable());

    private static readonly MethodInfo executeEnumerableMethodInfo
        = typeof(IDbCommandExecutor).GetTypeInfo().GetDeclaredMethod(nameof(IDbCommandExecutor.ExecuteEnumerable));

    private static readonly MethodInfo executeComplexMethodInfo
        = typeof(IDbCommandExecutor).GetTypeInfo().GetDeclaredMethod(nameof(IDbCommandExecutor.ExecuteComplex));

    private static readonly MethodInfo executeScalarMethodInfo
        = typeof(IDbCommandExecutor).GetTypeInfo().GetDeclaredMethod(nameof(IDbCommandExecutor.ExecuteScalar));

    private readonly ExpressionTranslatabilityAnalyzer translatabilityAnalyzer;
    private readonly IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory;
    private readonly MaterializerGeneratingExpressionVisitor materializerGeneratingExpressionVisitor;

    private static readonly RecursiveSubqueryAliasDecoratingExpressionVisitor recursiveSubqueryAliasDecoratingExpressionVisitor = new();

    public QueryCompilingExpressionVisitor(
        ExpressionTranslatabilityAnalyzer translatabilityAnalyzer,
        IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory,
        MaterializerGeneratingExpressionVisitor materializerGeneratingExpressionVisitor)
    {
        this.translatabilityAnalyzer = translatabilityAnalyzer ?? throw new ArgumentNullException(nameof(translatabilityAnalyzer));
        this.queryTranslatingExpressionVisitorFactory = queryTranslatingExpressionVisitorFactory ?? throw new ArgumentNullException(nameof(queryTranslatingExpressionVisitorFactory));
        this.materializerGeneratingExpressionVisitor = materializerGeneratingExpressionVisitor ?? throw new ArgumentNullException(nameof(materializerGeneratingExpressionVisitor));
    }

    public override Expression Visit(Expression node)
    {
        switch (node)
        {
            case EnumerableRelationalQueryExpression enumerableRelationalQueryExpression:
            {
                var selectExpression
                    = recursiveSubqueryAliasDecoratingExpressionVisitor
                        .VisitAndConvert(
                            enumerableRelationalQueryExpression.SelectExpression,
                            nameof(Visit));

                var commandBuilderLambda = queryTranslatingExpressionVisitorFactory.Create().Translate(selectExpression);
                var sequenceType = node.Type.GetSequenceType();
                var materializer = Visit(materializerGeneratingExpressionVisitor.Visit(selectExpression));

                return Expression.Call(
                    (enumerableRelationalQueryExpression.TransformationMethod
                        ?? asQueryableMethodInfo.MakeGenericMethod(sequenceType)),
                    Expression.Call(
                        ExecutionContextParameters.DbCommandExecutor,
                        executeEnumerableMethodInfo.MakeGenericMethod(sequenceType),
                        commandBuilderLambda,
                        materializer));
            }

            case SingleValueRelationalQueryExpression singleValueRelationalQueryExpression:
            {
                var selectExpression
                    = recursiveSubqueryAliasDecoratingExpressionVisitor
                        .VisitAndConvert(
                            singleValueRelationalQueryExpression.SelectExpression,
                            nameof(Visit));

                var commandBuilderLambda = queryTranslatingExpressionVisitorFactory.Create().Translate(selectExpression);
                var materializer = Visit(materializerGeneratingExpressionVisitor.Visit(selectExpression));

                return singleValueRelationalQueryExpression.Type.IsScalarType()
                    ? Expression.Call(
                        ExecutionContextParameters.DbCommandExecutor,
                        executeScalarMethodInfo.MakeGenericMethod(node.Type),
                        commandBuilderLambda)
                    : Expression.Call(
                        ExecutionContextParameters.DbCommandExecutor,
                        executeComplexMethodInfo.MakeGenericMethod(node.Type),
                        commandBuilderLambda,
                        materializer);
            }

            default:
            {
                return base.Visit(node);
            }
        }
    }

    private class RecursiveSubqueryAliasDecoratingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            if (node is EnumerableRelationalQueryExpression enumerableRelationalQueryExpression)
            {
                var selectExpression = VisitAndConvert(enumerableRelationalQueryExpression.SelectExpression, nameof(VisitExtension));

                selectExpression = HandleSelectExpression(selectExpression);

                return enumerableRelationalQueryExpression.UpdateSelectExpression(selectExpression);
            }
            if (node is SingleValueRelationalQueryExpression singleValueRelationalQueryExpression
                && !node.Type.IsScalarType())
            {
                var selectExpression = VisitAndConvert(singleValueRelationalQueryExpression.SelectExpression, nameof(VisitExtension));

                selectExpression = HandleSelectExpression(selectExpression);

                return singleValueRelationalQueryExpression.UpdateSelectExpression(selectExpression);
            }

            return base.VisitExtension(node);
        }

        private static SelectExpression HandleSelectExpression(SelectExpression selectExpression)
        {
            if (selectExpression.Projection is not ServerProjectionExpression)
            {
                return selectExpression;
            }

            var projection = selectExpression.Projection.Flatten().Body;
            var leafGatherer = new ProjectionLeafGatheringExpressionVisitor();
            leafGatherer.Visit(projection);

            if (leafGatherer.GatheredExpressions.Count == 1
                && string.IsNullOrEmpty(leafGatherer.GatheredExpressions.Keys.Single())
                && !(projection is SqlColumnExpression || projection is SqlAliasExpression))
            {
                selectExpression
                    = selectExpression.UpdateProjection(
                        new ServerProjectionExpression(
                            new SqlAliasExpression(
                                selectExpression.Projection.ResultLambda.Body,
                                "$c")));
            }

            return selectExpression;
        }
    }
}
