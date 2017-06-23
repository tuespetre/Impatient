using Impatient.Query.Expressions;
using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Infrastructure
{
    public class DefaultScalarReadValueExpressionFactory : IReadValueExpressionFactory
    {
        private static readonly TypeInfo dbDataReaderTypeInfo
            = typeof(DbDataReader).GetTypeInfo();

        private static readonly MethodInfo getFieldValueMethodInfo
            = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.GetFieldValue));

        private static readonly MethodInfo isDBNullMethodInfo
            = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

        public bool CanReadType(Type type)
        {
            return type.IsScalarType();
        }

        public Expression CreateExpression(Expression source, Expression reader, int index)
        {
            var readValueExpression
                = Expression.Call(
                    reader,
                    getFieldValueMethodInfo.MakeGenericMethod(source.Type),
                    Expression.Constant(index));

            if (source is SqlColumnExpression sqlColumnExpression
                && !sqlColumnExpression.IsNullable)
            {
                return readValueExpression;
            }

            return Expression.Condition(
                Expression.Call(
                    reader,
                    isDBNullMethodInfo,
                    Expression.Constant(index)),
                Expression.Default(source.Type),
                readValueExpression);
        }
    }
}
