using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors
{
    public class QueryCompilingExpressionVisitor : ExpressionVisitor
    {
        private readonly IImpatientExpressionVisitorProvider expressionVisitorProvider;
        private readonly ParameterExpression queryProviderParameter;

        public QueryCompilingExpressionVisitor(
            IImpatientExpressionVisitorProvider expressionVisitorProvider,
            ParameterExpression queryProviderParameter)
        {
            this.expressionVisitorProvider = expressionVisitorProvider ?? throw new ArgumentNullException(nameof(expressionVisitorProvider));
            this.queryProviderParameter = queryProviderParameter ?? throw new ArgumentNullException(nameof(queryProviderParameter));
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case EnumerableRelationalQueryExpression enumerableRelationalQueryExpression:
                {
                    var selectExpression = enumerableRelationalQueryExpression.SelectExpression;
                    var translator = expressionVisitorProvider.QueryTranslatingExpressionVisitor;                    
                    var commandBuilderLambda = translator.Translate(selectExpression);
                    var sequenceType = node.Type.GetSequenceType();

                    return Expression.Call(
                        (enumerableRelationalQueryExpression.TransformationMethod 
                            ?? asQueryableMethodInfo.MakeGenericMethod(sequenceType)),
                        Expression.Call(
                            executeEnumerableMethodInfo.MakeGenericMethod(sequenceType),
                            queryProviderParameter,
                            commandBuilderLambda,
                            Visit(GenerateMaterializer(selectExpression))));
                }

                case SingleValueRelationalQueryExpression singleValueRelationalQueryExpression:
                {
                    var selectExpression = singleValueRelationalQueryExpression.SelectExpression;
                    var translator = expressionVisitorProvider.QueryTranslatingExpressionVisitor;
                    var commandBuilderLambda = translator.Translate(selectExpression);

                    return singleValueRelationalQueryExpression.Type.IsScalarType()
                        ? Expression.Call(
                            executeScalarMethodInfo.MakeGenericMethod(node.Type),
                            queryProviderParameter,
                            commandBuilderLambda)
                        : Expression.Call(
                            executeComplexMethodInfo.MakeGenericMethod(node.Type),
                            queryProviderParameter,
                            commandBuilderLambda,
                            Visit(GenerateMaterializer(selectExpression)));
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }

        private static readonly MethodInfo asQueryableMethodInfo
            = ImpatientExtensions.GetGenericMethodDefinition((IEnumerable<object> e) => e.AsQueryable());

        private static readonly MethodInfo executeEnumerableMethodInfo
            = typeof(QueryCompilingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ExecuteEnumerable));

        private static readonly MethodInfo executeComplexMethodInfo
            = typeof(QueryCompilingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ExecuteComplex));

        private static readonly MethodInfo executeScalarMethodInfo
            = typeof(QueryCompilingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ExecuteScalar));

        private static IEnumerable<TElement> ExecuteEnumerable<TElement>(
            ImpatientQueryProvider queryProvider,
            Action<DbCommand> commandBuilder,
            Func<DbDataReader, TElement> materializer)
        {
            using (var connection = queryProvider.ConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                commandBuilder(command);

                queryProvider.DbCommandInterceptor?.Invoke(command);

                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        yield return materializer(reader);
                    }
                }
            }
        }

        private static TResult ExecuteComplex<TResult>(
            ImpatientQueryProvider queryProvider,
            Action<DbCommand> commandBuilder,
            Func<DbDataReader, TResult> materializer)
        {
            using (var connection = queryProvider.ConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                commandBuilder(command);

                queryProvider.DbCommandInterceptor?.Invoke(command);

                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    // TODO: Can the logic really be this simple?
                    // - Related to 'DefaultIfEmpty' and '___OrDefault' behavior
                    // - Currently relying on the queries themselves to supply:
                    //   - The default value, through the materializer
                    //   - Exceptions for First/Single/SingleOrDefault/Last/ElementAt

                    reader.Read();

                    return materializer(reader);
                }
            }
        }

        private static TResult ExecuteScalar<TResult>(
            ImpatientQueryProvider queryProvider,
            Action<DbCommand> commandBuilder)
        {
            using (var connection = queryProvider.ConnectionFactory.CreateConnection())
            using (var command = connection.CreateCommand())
            {
                commandBuilder(command);

                queryProvider.DbCommandInterceptor?.Invoke(command);

                connection.Open();

                return (TResult)command.ExecuteScalar();
            }
        }

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

            var readerParameter = Expression.Parameter(typeof(DbDataReader));

            return Expression.Lambda(
                MaterializeProjection(
                    selectExpression.Projection,
                    new MaterializerBuildingExpressionVisitor(expressionVisitorProvider, readerParameter)), 
                readerParameter);
        }

        private class MaterializerBuildingExpressionVisitor : ProjectionExpressionVisitor
        {
            private static readonly MethodInfo isDBNullMethodInfo
                = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

            private readonly IImpatientExpressionVisitorProvider expressionVisitorProvider;
            private readonly ParameterExpression readerParameter;
            private int readerIndex;

            private static readonly IReadValueExpressionFactory[] readValueExpressionFactories =
            {
                new DefaultScalarReadValueExpressionFactory(),
                new SqlServerForJsonReadValueExpressionFactory(),
            };

            public MaterializerBuildingExpressionVisitor(
                IImpatientExpressionVisitorProvider expressionVisitorProvider,
                ParameterExpression readerParameter)
            {
                this.expressionVisitorProvider = expressionVisitorProvider;
                this.readerParameter = readerParameter;
            }

            protected override Expression VisitLeaf(Expression node)
            {
                if (expressionVisitorProvider.TranslatabilityAnalyzingExpressionVisitor.Visit(node) is TranslatableExpression)
                {
                    return readValueExpressionFactories
                        .First(f => f.CanReadType(node.Type))
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
    }
}
