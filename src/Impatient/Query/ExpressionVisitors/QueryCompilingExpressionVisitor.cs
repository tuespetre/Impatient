using Impatient.Query.Expressions;
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
        private readonly ParameterExpression queryProviderParameter;

        public QueryCompilingExpressionVisitor(ParameterExpression queryProviderParameter)
        {
            this.queryProviderParameter = queryProviderParameter ?? throw new ArgumentNullException(nameof(queryProviderParameter));
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case EnumerableRelationalQueryExpression enumerableRelationalQueryExpression:
                {
                    var translator = new QueryTranslatingExpressionVisitor();

                    var (materializerLambda, commandBuilderLambda)
                        = translator.Translate(
                            enumerableRelationalQueryExpression.SelectExpression);

                    var sequenceType = node.Type.GetSequenceType();

                    return Expression.Call(
                        asQueryableMethodInfo.MakeGenericMethod(sequenceType),
                        Expression.Call(
                            executeEnumerableMethodInfo.MakeGenericMethod(sequenceType),
                            queryProviderParameter,
                            commandBuilderLambda,
                            Visit(materializerLambda)));
                }

                case SingleValueRelationalQueryExpression singleValueRelationalQueryExpression:
                {
                    var translator = new QueryTranslatingExpressionVisitor();

                    var (materializerLambda, commandBuilderLambda)
                        = translator.Translate(
                            singleValueRelationalQueryExpression.SelectExpression);

                    return singleValueRelationalQueryExpression.Type.IsScalarType()
                        ? Expression.Call(
                            executeScalarMethodInfo.MakeGenericMethod(node.Type),
                            queryProviderParameter,
                            commandBuilderLambda)
                        : Expression.Call(
                            executeComplexMethodInfo.MakeGenericMethod(node.Type),
                            queryProviderParameter,
                            commandBuilderLambda,
                            Visit(materializerLambda));
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
    }
}
