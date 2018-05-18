using Impatient.Extensions;
using Impatient.Query.Expressions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Tests.Utilities
{
    public static class QueryExpressionHelper
    {
        public static Expression CreateQueryExpression<TElement>()
        {
            var type = typeof(TElement);

            var annotation = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();

            var table = new BaseTableExpression(
                annotation?.Schema ?? "dbo",
                annotation?.Name ?? type.Name,
                (annotation?.Name ?? type.Name).ToLower().First().ToString(),
                type);

            return new EnumerableRelationalQueryExpression(
                new SelectExpression(
                    new ServerProjectionExpression(
                        Expression.MemberInit(
                            Expression.New(type),
                            from property in type.GetTypeInfo().DeclaredProperties
                            where property.PropertyType.IsScalarType()
                            let nullable =
                                (property.PropertyType.IsNullableType())
                                || (!property.PropertyType.GetTypeInfo().IsValueType
                                    && property.GetCustomAttribute<RequiredAttribute>() == null)
                            let column = new SqlColumnExpression(table, property.Name, property.PropertyType, nullable, null)
                            select Expression.Bind(property, column))),
                    table));
        }

        public static LambdaExpression GetExpression<TSource, TResult>(Expression<Func<TSource, TResult>> expression)
        {
            return expression;
        }
    }
}
