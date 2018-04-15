using Impatient.Query.Expressions;
using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Infrastructure
{
    public static class DefaultScalarValueReader
    {
        public static TValue ReadNonNullable<TValue>(DbDataReader reader, int index)
        {
            return reader.GetFieldValue<TValue>(index);
        }

        public static TValue ReadNullable<TValue>(DbDataReader reader, int index)
        {
            if (reader.IsDBNull(index))
            {
                return default;
            }
            else
            {
                return reader.GetFieldValue<TValue>(index);
            }
        }

        public static TValue ReadNonNullableEnum<TValue>(DbDataReader reader, int index)
        {
            return (TValue)Enum.ToObject(typeof(TValue), reader.GetValue(index));
        }

        public static TNullable ReadNullableEnum<TNullable, TValue>(DbDataReader reader, int index)
        {
            if (reader.IsDBNull(index))
            {
                return default;
            }
            else
            {
                return (TNullable)Enum.ToObject(typeof(TValue), reader.GetValue(index));
            }
        }
    }

    public class DefaultScalarReadValueExpressionFactory : IReadValueExpressionFactory
    {
        private static readonly MethodInfo readNonNullableMethodInfo
            = typeof(DefaultScalarValueReader).GetTypeInfo()
                .GetDeclaredMethod(nameof(DefaultScalarValueReader.ReadNonNullable));

        private static readonly MethodInfo readNullableMethodInfo
            = typeof(DefaultScalarValueReader).GetTypeInfo()
                .GetDeclaredMethod(nameof(DefaultScalarValueReader.ReadNullable));

        private static readonly MethodInfo readNonNullableEnumMethodInfo
            = typeof(DefaultScalarValueReader).GetTypeInfo()
                .GetDeclaredMethod(nameof(DefaultScalarValueReader.ReadNonNullableEnum));

        private static readonly MethodInfo readNullableEnumMethodInfo
            = typeof(DefaultScalarValueReader).GetTypeInfo()
                .GetDeclaredMethod(nameof(DefaultScalarValueReader.ReadNullableEnum));

        public bool CanReadExpression(Expression expression)
        {
            return expression.Type.IsScalarType();
        }

        public Expression CreateExpression(Expression source, Expression reader, int index)
        {
            var unwrappedType = source.Type.UnwrapNullableType();

            if (unwrappedType.GetTypeInfo().IsEnum)
            {
                return Expression.Call(
                    source is SqlColumnExpression column && !column.IsNullable
                        ? readNonNullableEnumMethodInfo.MakeGenericMethod(unwrappedType)
                        : readNullableEnumMethodInfo.MakeGenericMethod(source.Type, unwrappedType),
                    reader,
                    Expression.Constant(index));
            }
            else
            {
                return Expression.Call(
                    source is SqlColumnExpression column && !column.IsNullable
                        ? readNonNullableMethodInfo.MakeGenericMethod(source.Type)
                        : readNullableMethodInfo.MakeGenericMethod(source.Type),
                    reader,
                    Expression.Constant(index));
            }
        }
    }
}
