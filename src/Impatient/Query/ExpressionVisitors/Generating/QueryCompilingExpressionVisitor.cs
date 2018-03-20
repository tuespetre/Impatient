using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Generating
{
    public class QueryCompilingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo asQueryableMethodInfo
            = ImpatientExtensions.GetGenericMethodDefinition((IEnumerable<object> e) => e.AsQueryable());

        private static readonly MethodInfo executeEnumerableMethodInfo
            = typeof(IDbCommandExecutor).GetTypeInfo().GetDeclaredMethod(nameof(IDbCommandExecutor.ExecuteEnumerable));

        private static readonly MethodInfo executeComplexMethodInfo
            = typeof(IDbCommandExecutor).GetTypeInfo().GetDeclaredMethod(nameof(IDbCommandExecutor.ExecuteComplex));

        private static readonly MethodInfo executeScalarMethodInfo
            = typeof(IDbCommandExecutor).GetTypeInfo().GetDeclaredMethod(nameof(IDbCommandExecutor.ExecuteScalar));

        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityVisitor;
        private readonly IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory;
        private readonly ParameterExpression executorParameter;

        public QueryCompilingExpressionVisitor(
            TranslatabilityAnalyzingExpressionVisitor translatabilityVisitor,
            IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory,
            ParameterExpression executionContextParameter)
        {
            this.translatabilityVisitor = translatabilityVisitor ?? throw new ArgumentNullException(nameof(translatabilityVisitor));
            this.queryTranslatingExpressionVisitorFactory = queryTranslatingExpressionVisitorFactory ?? throw new ArgumentNullException(nameof(queryTranslatingExpressionVisitorFactory));
            this.executorParameter = executionContextParameter ?? throw new ArgumentNullException(nameof(executionContextParameter));
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

                    return Expression.Call(
                        (enumerableRelationalQueryExpression.TransformationMethod
                            ?? asQueryableMethodInfo.MakeGenericMethod(sequenceType)),
                        Expression.Call(
                            executorParameter,
                            executeEnumerableMethodInfo.MakeGenericMethod(sequenceType),
                            commandBuilderLambda,
                            Visit(GenerateMaterializer(selectExpression))));
                }

                case SingleValueRelationalQueryExpression singleValueRelationalQueryExpression:
                {
                    var selectExpression = singleValueRelationalQueryExpression.SelectExpression;
                    var commandBuilderLambda = queryTranslatingExpressionVisitorFactory.Create().Translate(selectExpression);

                    return singleValueRelationalQueryExpression.Type.IsScalarType()
                        ? Expression.Call(
                            executorParameter,
                            executeScalarMethodInfo.MakeGenericMethod(node.Type),
                            commandBuilderLambda)
                        : Expression.Call(
                            executorParameter,
                            executeComplexMethodInfo.MakeGenericMethod(node.Type),
                            commandBuilderLambda,
                            Visit(GenerateMaterializer(selectExpression)));
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }

        #region Materializer generation

        private LambdaExpression GenerateMaterializer(SelectExpression selectExpression)
        {
            Expression MaterializeProjection(ProjectionExpression projection, MaterializerBuildingExpressionVisitor visitor)
            {
                switch (projection)
                {
                    case ServerProjectionExpression serverProjectionExpression:
                    {
                        return visitor.Visit(serverProjectionExpression.ResultLambda.Body);
                    }

                    case ClientProjectionExpression clientProjectionExpression:
                    {
                        var server = clientProjectionExpression.ServerProjection;
                        var result = clientProjectionExpression.ResultLambda;

                        return Expression.Invoke(result, MaterializeProjection(server, visitor));
                    }

                    case CompositeProjectionExpression compositeProjectionExpression:
                    {
                        var outer = MaterializeProjection(compositeProjectionExpression.OuterProjection, visitor);
                        var inner = MaterializeProjection(compositeProjectionExpression.InnerProjection, visitor);
                        var result = compositeProjectionExpression.ResultLambda;

                        return Expression.Invoke(result, outer, inner);
                    }

                    default:
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            {
                var readerParameter = Expression.Parameter(typeof(DbDataReader));
                var visitor = new MaterializerBuildingExpressionVisitor(translatabilityVisitor, readerParameter);

                return Expression.Lambda(MaterializeProjection(selectExpression.Projection, visitor), readerParameter);
            }
        }

        private class MaterializerBuildingExpressionVisitor : ProjectionExpressionVisitor
        {
            private static readonly MethodInfo isDBNullMethodInfo
                = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

            private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityVisitor;
            private readonly ParameterExpression readerParameter;
            private int readerIndex;

            private static readonly IReadValueExpressionFactory[] readValueExpressionFactories =
            {
                new DefaultScalarReadValueExpressionFactory(),
                new SqlServerForJsonReadValueExpressionFactory(),
            };

            public MaterializerBuildingExpressionVisitor(
                TranslatabilityAnalyzingExpressionVisitor translatabilityVisitor,
                ParameterExpression readerParameter)
            {
                this.translatabilityVisitor = translatabilityVisitor;
                this.readerParameter = readerParameter;
            }

            protected override Expression VisitLeaf(Expression node)
            {
                if (translatabilityVisitor.Visit(node) is TranslatableExpression)
                {
                    return readValueExpressionFactories
                        .First(f => f.CanReadExpression(node))
                        .CreateExpression(node, readerParameter, readerIndex++);
                }

                return node;
            }

            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case DefaultIfEmptyExpression defaultIfEmptyExpression:
                    {
                        return Expression.Condition(
                            test: Expression.Call(
                                readerParameter,
                                isDBNullMethodInfo,
                                Expression.Constant(readerIndex++)),
                            ifTrue: Expression.Default(defaultIfEmptyExpression.Type),
                            ifFalse: Visit(defaultIfEmptyExpression.Expression));
                    }

                    case PolymorphicExpression polymorphicExpression:
                    {
                        var row = Visit(polymorphicExpression.Row);
                        var descriptors = polymorphicExpression.Descriptors.ToArray();
                        var result = Expression.Default(polymorphicExpression.Type) as Expression;

                        for (var i = 0; i < descriptors.Length; i++)
                        {
                            result = Expression.Condition(
                                test: descriptors[i].Test.ExpandParameters(row),
                                ifTrue: descriptors[i].Materializer.ExpandParameters(row),
                                ifFalse: result,
                                type: polymorphicExpression.Type);
                        }

                        return result;
                    }

                    case UnaryExpression unaryExpression
                    when unaryExpression.NodeType == ExpressionType.Convert:
                    {
                        return unaryExpression.Update(Visit(unaryExpression.Operand));
                    }

                    default:
                    {
                        return base.Visit(node);
                    }
                }
            }
        }

        #endregion
    }
}
