using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Generating
{
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

        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityVisitor;
        private readonly IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory;
        private readonly MaterializerGeneratingExpressionVisitor materializerGeneratingExpressionVisitor;
        private readonly ParameterExpression executionContextParameter;

        public QueryCompilingExpressionVisitor(
            TranslatabilityAnalyzingExpressionVisitor translatabilityVisitor,
            IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory,
            MaterializerGeneratingExpressionVisitor materializerGeneratingExpressionVisitor,
            ParameterExpression executionContextParameter)
        {
            this.translatabilityVisitor = translatabilityVisitor ?? throw new ArgumentNullException(nameof(translatabilityVisitor));
            this.queryTranslatingExpressionVisitorFactory = queryTranslatingExpressionVisitorFactory ?? throw new ArgumentNullException(nameof(queryTranslatingExpressionVisitorFactory));
            this.materializerGeneratingExpressionVisitor = materializerGeneratingExpressionVisitor ?? throw new ArgumentNullException(nameof(materializerGeneratingExpressionVisitor));
            this.executionContextParameter = executionContextParameter ?? throw new ArgumentNullException(nameof(executionContextParameter));
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case EnumerableRelationalQueryExpression enumerableRelationalQueryExpression:
                {
                    var selectExpression = enumerableRelationalQueryExpression.SelectExpression;
                    var commandBuilderLambda = queryTranslatingExpressionVisitorFactory.Create().Translate(selectExpression);
                    var sequenceType = node.Type.GetSequenceType();
                    var materializer = Visit(materializerGeneratingExpressionVisitor.Visit(selectExpression));

                    return Expression.Call(
                        (enumerableRelationalQueryExpression.TransformationMethod
                            ?? asQueryableMethodInfo.MakeGenericMethod(sequenceType)),
                        Expression.Call(
                            executionContextParameter,
                            executeEnumerableMethodInfo.MakeGenericMethod(sequenceType),
                            commandBuilderLambda,
                            materializer));
                }

                case SingleValueRelationalQueryExpression singleValueRelationalQueryExpression:
                {
                    var selectExpression = singleValueRelationalQueryExpression.SelectExpression;
                    var commandBuilderLambda = queryTranslatingExpressionVisitorFactory.Create().Translate(selectExpression);
                    var materializer = Visit(materializerGeneratingExpressionVisitor.Visit(selectExpression));

                    return singleValueRelationalQueryExpression.Type.IsScalarType()
                        ? Expression.Call(
                            executionContextParameter,
                            executeScalarMethodInfo.MakeGenericMethod(node.Type),
                            commandBuilderLambda)
                        : Expression.Call(
                            executionContextParameter,
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
    }
}
