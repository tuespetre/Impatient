using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Enumerable;

namespace Impatient.Query.Infrastructure
{
    public class SqlServerForJsonReadValueExpressionFactory : IReadValueExpressionFactory
    {
        private static readonly TypeInfo dbDataReaderTypeInfo
            = typeof(DbDataReader).GetTypeInfo();

        private static readonly MethodInfo getFieldValueMethodInfo
            = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.GetFieldValue));

        private static readonly MethodInfo isDBNullMethodInfo
            = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

        private static readonly MethodInfo enumerableEmptyMethodInfo
            = ImpatientExtensions.GetGenericMethodDefinition((object o) => Empty<object>());

        private static readonly MethodInfo asQueryableMethodInfo
            = ImpatientExtensions.GetGenericMethodDefinition((IEnumerable<object> e) => e.AsQueryable());

        private static readonly MethodInfo jsonConvertDeserializeObjectMethodInfo
            = ImpatientExtensions.GetGenericMethodDefinition((string s) => JsonConvert.DeserializeObject<object>(s));

        public bool CanReadType(Type type)
        {
            return !type.IsScalarType();
        }

        public Expression CreateExpression(Expression source, Expression reader, int index)
        {
            var type = source.Type;
            var sequenceType = type.GetSequenceType();
            var defaultValue = Expression.Default(type) as Expression;

            if (type.IsSequenceType())
            {
                if (type.IsArray)
                {
                    defaultValue = Expression.NewArrayInit(type.GetElementType());
                }
                else if (type.IsGenericType(typeof(List<>)))
                {
                    defaultValue = Expression.New(type);
                }
                else
                {
                    type = type.FindGenericType(typeof(IEnumerable<>));
                    defaultValue = Expression.Call(enumerableEmptyMethodInfo.MakeGenericMethod(sequenceType));
                }

                if (sequenceType.IsScalarType())
                {
                    // TODO: Handle sequences of scalar types with FOR JSON
                    // - Use a JsonTextReader to stream through the text
                    // - while (reader.Read())
                    // - (StartArray)(1)
                    // - (StartObject -> PropertyName -> String | Number -> EndObject)(*)
                    // - (EndArray)(1)
                }
            }

            var getFieldValueExpression
                = Expression.Call(
                    reader,
                    getFieldValueMethodInfo.MakeGenericMethod(typeof(string)),
                    Expression.Constant(index));

            var deserializerExpression
                = Expression.Call(
                    jsonConvertDeserializeObjectMethodInfo.MakeGenericMethod(type), 
                    getFieldValueExpression);

            var result
                = Expression.Condition(
                    Expression.Call(
                        reader,
                        isDBNullMethodInfo,
                        Expression.Constant(index)),
                    defaultValue,
                    deserializerExpression) as Expression;

            if (source.Type.IsGenericType(typeof(IQueryable<>)))
            {
                result 
                    = Expression.Call(
                        asQueryableMethodInfo.MakeGenericMethod(sequenceType), 
                        result);
            }

            return result;
        }
    }
}
