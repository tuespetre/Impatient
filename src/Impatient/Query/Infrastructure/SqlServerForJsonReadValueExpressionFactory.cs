using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Enumerable;

namespace Impatient.Query.Infrastructure
{
    public class SqlServerForJsonReadValueExpressionFactory : IReadValueExpressionFactory
    {
        #region reflection

        private static readonly MethodInfo dbDataReaderGetFieldValueMethodInfo
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetFieldValue));

        private static readonly MethodInfo dbDataReaderIsDBNullMethodInfo
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

        private static readonly MethodInfo enumerableEmptyMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((object o) => Empty<object>());

        private static readonly MethodInfo enumerableToArrayMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IEnumerable<object> e) => e.ToArray());

        private static readonly MethodInfo queryableAsQueryableMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IEnumerable<object> e) => e.AsQueryable());

        private static readonly ConstructorInfo jsonTextReaderConstructorInfo
            = typeof(JsonTextReader).GetConstructor(new[] { typeof(StringReader) });

        private static readonly MethodInfo jsonTextReaderReadMethodInfo
            = typeof(JsonTextReader).GetRuntimeMethod(nameof(JsonTextReader.Read), new Type[0]);

        private static readonly PropertyInfo jsonTextReaderTokenTypePropertyInfo
            = typeof(JsonTextReader).GetRuntimeProperty(nameof(JsonTextReader.TokenType));

        private static readonly PropertyInfo jsonTextReaderValuePropertyInfo
            = typeof(JsonTextReader).GetRuntimeProperty(nameof(JsonTextReader.Value));

        private static readonly PropertyInfo jsonTextReaderDateParseHandlingPropertyInfo
            = typeof(JsonTextReader).GetRuntimeProperty(nameof(JsonTextReader.DateParseHandling));

        private static readonly ConstructorInfo stringReaderConstructorInfo
            = typeof(StringReader).GetConstructor(new[] { typeof(string) });

        private static readonly MethodInfo disposableDisposeMethodInfo
            = typeof(IDisposable).GetTypeInfo().GetDeclaredMethod(nameof(IDisposable.Dispose));

        #endregion

        public bool CanReadExpression(Expression expression)
        {
            if (expression.Type.IsScalarType())
            {
                return false;
            }

            return true;
        }

        public Expression CreateExpression(Expression source, Expression reader, int index)
        {
            var jsonTextReaderVariable = Expression.Variable(typeof(JsonTextReader), "jsonTextReader");
            var resultVariable = Expression.Variable(source.Type, "result");

            var materializer
                = new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReaderVariable)
                    .Visit(source);

            return Expression.Condition(
                Expression.Call(reader, dbDataReaderIsDBNullMethodInfo, Expression.Constant(index)),
                CreateDefaultValueExpression(source.Type),
                Expression.Block(
                    variables: new[]
                    {
                        jsonTextReaderVariable
                    },
                    expressions: new Expression[]
                    {
                        Expression.Assign(
                            jsonTextReaderVariable,
                            Expression.New(
                                jsonTextReaderConstructorInfo,
                                Expression.New(
                                    stringReaderConstructorInfo,
                                    Expression.Call(
                                        reader,
                                        dbDataReaderGetFieldValueMethodInfo.MakeGenericMethod(typeof(string)),
                                        Expression.Constant(index))))),
                        Expression.TryFinally(
                            body: Expression.Block(
                                Expression.Assign(
                                    Expression.Property(jsonTextReaderVariable, jsonTextReaderDateParseHandlingPropertyInfo),
                                    Expression.Constant(DateParseHandling.None)),
                                materializer),
                            @finally: Expression.Call(
                                Expression.Convert(jsonTextReaderVariable, typeof(IDisposable)),
                                disposableDisposeMethodInfo)),
                    }));
        }

        private static Expression CreateSequenceExpression(Expression expression, Type type)
        {
            if (type.IsGenericType(typeof(IQueryable<>)))
            {
                var sequenceType = type.GetSequenceType();

                // Calling AsQueryable creates a self-referencing
                // EnumerableQuery whose inner list/array/etc. cannot
                // be accessed without reflection. We want other visitors
                // to have the chance to access it so we manually construct
                // the EnumerableQuery from a ConstantExpression.
                expression = expression.AsEnumerableQuery();

                if (type.IsGenericType(typeof(IOrderedQueryable<>)))
                {
                    expression
                        = Expression.New(
                            typeof(StubOrderedQueryableEnumerable<>)
                                .MakeGenericType(sequenceType)
                                .GetTypeInfo()
                                .DeclaredConstructors
                                .Single(),
                            expression);
                }
            }

            return expression;
        }

        private static Expression CreateDefaultValueExpression(Type type)
        {
            if (type.IsSequenceType())
            {
                if (type.IsArray)
                {
                    return Expression.NewArrayInit(type.GetElementType());
                }
                else if (type.GetTypeInfo().DeclaredConstructors.Any(c => c.GetParameters().Length == 0))
                {
                    return Expression.New(type);
                }
                else
                {
                    var sequenceType = type.GetSequenceType();

                    var defaultValue = Expression.Call(enumerableEmptyMethodInfo.MakeGenericMethod(sequenceType));

                    return CreateSequenceExpression(defaultValue, type);
                }
            }
            else
            {
                return Expression.Default(type);
            }
        }

        private static Expression ExtractProjectionExpression(Expression node)
        {
            switch (node)
            {
                case SqlAliasExpression sqlAliasExpression:
                {
                    return ExtractProjectionExpression(sqlAliasExpression.Expression);
                }

                case SqlColumnExpression sqlColumnExpression
                when sqlColumnExpression.Table is SubqueryTableExpression subqueryTableExpression:
                {
                    var body = subqueryTableExpression.Subquery.Projection.Flatten().Body;

                    if (body.TryResolvePath(sqlColumnExpression.ColumnName, out var resolved))
                    {
                        return ExtractProjectionExpression(resolved);
                    }

                    return ExtractProjectionExpression(body);
                }

                case EnumerableRelationalQueryExpression relationalQueryExpression:
                {
                    return relationalQueryExpression.SelectExpression.Projection.Flatten().Body;
                }

                case RelationalQueryExpression relationalQueryExpression:
                {
                    return ExtractProjectionExpression(relationalQueryExpression.SelectExpression.Projection.Flatten().Body);
                }

                case MethodCallExpression methodCallExpression
                when methodCallExpression.Method.Name == nameof(Queryable.FirstOrDefault):
                {
                    return ExtractProjectionExpression(methodCallExpression.Arguments[0]);
                }

                default:
                {
                    return node;
                }
            }
        }

        private class ComplexTypeMaterializerBuildingExpressionVisitor : ProjectionExpressionVisitor
        {
            private readonly ParameterExpression jsonTextReader;
            private readonly Expression readExpression;
            private readonly Expression currentTokenType;
            private int depth = 0;
            private bool extraProperties;

            public ComplexTypeMaterializerBuildingExpressionVisitor(ParameterExpression jsonTextReader)
            {
                this.jsonTextReader = jsonTextReader;

                readExpression = Expression.Call(jsonTextReader, jsonTextReaderReadMethodInfo);
                currentTokenType = Expression.Property(jsonTextReader, jsonTextReaderTokenTypePropertyInfo);
            }

            private Expression CurrentTokenIs(JsonToken token)
            {
                return Expression.Equal(currentTokenType, Expression.Constant(token));
            }

            protected override Expression VisitLeaf(Expression node)
            {
                if (node.Type.IsScalarType())
                {
                    return SqlServerJsonValueReader.CreateExpression(node.Type, jsonTextReader);
                }
                else if (node.Type.IsSequenceType())
                {
                    var sequenceType = node.Type.GetSequenceType();
                    var listVariable = Expression.Variable(typeof(List<>).MakeGenericType(sequenceType));
                    var listAddMethod = listVariable.Type.GetRuntimeMethod(nameof(List<object>.Add), new[] { sequenceType });
                    var breakLabelTarget = Expression.Label();

                    Expression readItem;

                    if (sequenceType.IsScalarType())
                    {
                        readItem = Expression.Block(new[]
                        {
                            Expression.Call(
                                listVariable,
                                listAddMethod,
                                SqlServerJsonValueReader.CreateExpression(sequenceType, jsonTextReader)),
                            readExpression, // EndObject
                            readExpression, // StartObject | EndArray
                        });
                    }
                    else if (sequenceType.IsSequenceType())
                    {
                        readItem = Expression.Block(new[]
                        {
                            readExpression, // PropertyName
                            Expression.Call(
                                listVariable,
                                listAddMethod,
                                new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReader)
                                    .Visit(ExtractProjectionExpression(node))),
                            readExpression, // EndObject
                            readExpression, // StartObject | EndArray
                        });
                    }
                    else
                    {
                        readItem = Expression.Block(new[]
                        {
                            Expression.Call(
                                listVariable,
                                listAddMethod,
                                new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReader)
                                    .Visit(ExtractProjectionExpression(node))),
                            readExpression, // EndObject
                            readExpression, // StartObject | EndArray
                        });
                    }

                    return Expression.Block(
                        variables: new[]
                        {
                            listVariable
                        },
                        expressions: new[]
                        {
                            Expression.Assign(listVariable, Expression.New(listVariable.Type)),
                            depth > 0 ? readExpression : Expression.Empty(), // PropertyName
                            readExpression, // StartArray
                            Expression.IfThen(
                                Expression.Not(CurrentTokenIs(JsonToken.Null)),
                                Expression.Block(
                                    readExpression, // StartObject | EndArray
                                    Expression.Loop(
                                        Expression.IfThenElse(
                                            test: CurrentTokenIs(JsonToken.EndArray),
                                            ifTrue: Expression.Break(breakLabelTarget),
                                            ifFalse: readItem),
                                        breakLabelTarget))),
                            node.Type.IsArray
                                ? Expression.Call(
                                    enumerableToArrayMethodInfo.MakeGenericMethod(sequenceType),
                                    listVariable)
                                : CreateSequenceExpression(listVariable, node.Type),
                        });
                }
                else
                {
                    return new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReader)
                        .Visit(ExtractProjectionExpression(node));
                }
            }

            private Expression MaterializeObject(Expression visited, bool skipReadingEndObject = false)
            {
                if (depth > 0 && !extraProperties)
                {
                    var temporaryVariableExpression = Expression.Variable(visited.Type);

                    return Expression.Block(
                        new[] { temporaryVariableExpression },
                        readExpression, // PropertyName
                        readExpression, // StartObject | Null
                        Expression.Condition(
                            test: CurrentTokenIs(JsonToken.Null),
                            ifTrue: Expression.Default(visited.Type),
                            ifFalse: skipReadingEndObject
                                ? visited
                                : Expression.Block(
                                    Expression.Assign(temporaryVariableExpression, visited),
                                    readExpression, // EndObject
                                    temporaryVariableExpression)));
                }

                return visited;
            }

            private static void SkipToEndOfObject(JsonTextReader reader)
            {
                var depth = reader.Depth;

                Scan:
                while (reader.TokenType != JsonToken.EndObject)
                {
                    reader.Read();
                }

                if (reader.Depth + 1 != depth)
                {
                    goto Scan;
                }
            }

            private Expression MaybeRead => depth > 0 && !extraProperties ? readExpression : Expression.Empty();

            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case DefaultIfEmptyExpression defaultIfEmptyExpression:
                    {
                        var flag = extraProperties;

                        extraProperties = true;

                        var visited = Visit(defaultIfEmptyExpression.Expression);

                        extraProperties = flag;

                        var variable = Expression.Variable(node.Type);

                        var expressions = new List<Expression>
                        {
                            readExpression, // PropertyName
                            readExpression, // Value (DefaultIfEmpty flag)
                            Expression.Condition(
                                CurrentTokenIs(JsonToken.Null),
                                Expression.Block(
                                    Expression.Call(
                                        GetType().GetMethod(nameof(SkipToEndOfObject), BindingFlags.Static | BindingFlags.NonPublic),
                                        jsonTextReader),
                                    Expression.Default(node.Type)),
                                Expression.Block(
                                    Expression.Assign(variable, visited),
                                    MaybeRead,
                                    variable)),
                        };

                        var block = Expression.Block(new[] { variable }, expressions);

                        return MaterializeObject(block, skipReadingEndObject: true);
                    }

                    case ExtraPropertiesExpression extraPropertiesExpression:
                    {
                        var flag = extraProperties;

                        extraProperties = true;

                        var visited = base.Visit(node);

                        extraProperties = flag;

                        return MaterializeObject(visited);
                    }

                    case NewExpression newExpression when IsNotLeaf(newExpression):
                    case MemberInitExpression memberInitExpression when IsNotLeaf(memberInitExpression):
                    {
                        var flag = extraProperties;

                        depth++;

                        extraProperties = false;

                        var visited = base.Visit(node);

                        extraProperties = flag;

                        depth--;

                        return MaterializeObject(visited);
                    }

                    case PolymorphicExpression polymorphicExpression:
                    {
                        var variables = new List<ParameterExpression>();
                        var expressions = new List<Expression>();

                        var rowValue = polymorphicExpression.Row;
                        var rowVariable = Expression.Variable(rowValue.Type, "row");
                        var rowParameterExpansion = (Expression)rowVariable;

                        var flag = extraProperties;

                        depth++;

                        if (rowValue is ExtraPropertiesExpression extraPropertiesExpression)
                        {
                            extraProperties = false;

                            var properties = new List<Expression>();

                            for (var i = 0; i < extraPropertiesExpression.Names.Count; i++)
                            {
                                var propertyName = extraPropertiesExpression.Names[i];
                                var propertyValue = Visit(extraPropertiesExpression.Properties[i]);
                                var propertyVariable = Expression.Variable(propertyValue.Type, propertyName);

                                variables.Add(propertyVariable);
                                properties.Add(propertyVariable);
                                expressions.Add(Expression.Assign(propertyVariable, propertyValue));
                            }

                            extraProperties = true;

                            rowValue = Visit(extraPropertiesExpression.Expression);
                            rowVariable = Expression.Variable(rowValue.Type, "row");
                            rowParameterExpansion = extraPropertiesExpression.Update(rowVariable, properties);

                        }
                        else
                        {
                            extraProperties = true;

                            rowValue = Visit(rowValue);
                        }

                        extraProperties = flag;

                        depth--;

                        variables.Add(rowVariable);

                        expressions.Add(Expression.Assign(rowVariable, rowValue));

                        var result = Expression.Default(polymorphicExpression.Type) as Expression;

                        foreach (var descriptor in polymorphicExpression.Descriptors)
                        {
                            var test = descriptor.Test.ExpandParameters(rowParameterExpansion);
                            var materializer = descriptor.Materializer.ExpandParameters(rowParameterExpansion);
                            var expansion = Expression.Convert(materializer, polymorphicExpression.Type);

                            result = Expression.Condition(test, expansion, result, polymorphicExpression.Type);
                        }

                        expressions.Add(result);

                        return MaterializeObject(Expression.Block(variables, expressions));
                    }

                    default:
                    {
                        return base.Visit(node);
                    }
                }
            }
        }
    }

    internal static class SqlServerJsonValueReader
    {
        private static void ReadPropertyName(JsonTextReader reader)
        {
            reader.Read();

            Debug.Assert(reader.TokenType == JsonToken.PropertyName);
        }

        public static string ReadString(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            return reader.ReadAsString();
        }

        public static byte[] ReadBytes(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            return reader.ReadAsBytes();
        }

        public static byte ReadByte(JsonTextReader reader)
        {
            return ReadNullableByte(reader).GetValueOrDefault();
        }

        public static byte? ReadNullableByte(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            return unchecked((byte?)reader.ReadAsInt32());
        }

        public static short ReadShort(JsonTextReader reader)
        {
            return ReadNullableShort(reader).GetValueOrDefault();
        }

        public static short? ReadNullableShort(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            return unchecked((short?)reader.ReadAsInt32());
        }

        public static int ReadInteger(JsonTextReader reader)
        {
            return ReadNullableInteger(reader).GetValueOrDefault();
        }

        public static int? ReadNullableInteger(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            return reader.ReadAsInt32();
        }

        public static long ReadLong(JsonTextReader reader)
        {
            return ReadNullableLong(reader).GetValueOrDefault();
        }

        public static long? ReadNullableLong(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            return (long?)reader.ReadAsDouble();
        }

        public static decimal ReadDecimal(JsonTextReader reader)
        {
            return ReadNullableDecimal(reader).GetValueOrDefault();
        }

        public static decimal? ReadNullableDecimal(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            return reader.ReadAsDecimal();
        }

        public static float ReadFloat(JsonTextReader reader)
        {
            return ReadNullableFloat(reader).GetValueOrDefault();
        }

        public static float? ReadNullableFloat(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            return (float?)reader.ReadAsDouble();
        }

        public static double ReadDouble(JsonTextReader reader)
        {
            return ReadNullableDouble(reader).GetValueOrDefault();
        }

        public static double? ReadNullableDouble(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            return reader.ReadAsDouble();
        }

        public static bool ReadBoolean(JsonTextReader reader)
        {
            return ReadNullableBoolean(reader).GetValueOrDefault();
        }

        public static bool? ReadNullableBoolean(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            return reader.ReadAsBoolean();
        }

        public static Guid ReadGuid(JsonTextReader reader)
        {
            return ReadNullableGuid(reader).GetValueOrDefault();
        }

        public static Guid? ReadNullableGuid(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            var value = reader.ReadAsString();

            if (Guid.TryParse(value, out var result))
            {
                return result;
            }

            return default;
        }

        public static DateTime ReadDateTime(JsonTextReader reader)
        {
            return ReadNullableDateTime(reader).GetValueOrDefault();
        }

        public static DateTime? ReadNullableDateTime(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            var value = reader.ReadAsString();

            if (DateTime.TryParse(value, out var result))
            {
                return result;
            }

            return default;
        }

        public static DateTimeOffset ReadDateTimeOffset(JsonTextReader reader)
        {
            return ReadNullableDateTimeOffset(reader).GetValueOrDefault();
        }

        public static DateTimeOffset? ReadNullableDateTimeOffset(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            var value = reader.ReadAsString();

            if (DateTimeOffset.TryParse(value, out var result))
            {
                return result;
            }

            return default;
        }

        public static TimeSpan ReadTimeSpan(JsonTextReader reader)
        {
            return ReadNullableTimeSpan(reader).GetValueOrDefault();
        }

        public static TimeSpan? ReadNullableTimeSpan(JsonTextReader reader)
        {
            ReadPropertyName(reader);

            var value = reader.ReadAsString();

            if (TimeSpan.TryParse(value, out var result))
            {
                return result;
            }

            return default;
        }

        public static TEnum ReadEnum<TEnum>(JsonTextReader reader) where TEnum : struct
        {
            return ReadNullableEnum<TEnum>(reader).GetValueOrDefault();
        }

        public static TEnum? ReadNullableEnum<TEnum>(JsonTextReader reader) where TEnum : struct
        {
            ReadPropertyName(reader);

            reader.Read();

            if (reader.Value == null)
            {
                return default;
            }
            else
            {
                return (TEnum?)Enum.ToObject(typeof(TEnum), reader.Value);
            }
        }

        private static Expression MakeCall(string method, Expression reader)
        {
            return Expression.Call(typeof(SqlServerJsonValueReader).GetMethod(method), reader);
        }

        public static Expression CreateExpression(Type type, Expression reader)
        {
            Debug.Assert(reader.Type == typeof(JsonTextReader));

            if (type == typeof(string))
            {
                return MakeCall(nameof(ReadString), reader);
            }
            else if (type == typeof(byte[]))
            {
                return MakeCall(nameof(ReadBytes), reader);
            }
            else if (type == typeof(byte))
            {
                return MakeCall(nameof(ReadByte), reader);
            }
            else if (type == typeof(short))
            {
                return MakeCall(nameof(ReadShort), reader);
            }
            else if (type == typeof(int))
            {
                return MakeCall(nameof(ReadInteger), reader);
            }
            else if (type == typeof(long))
            {
                return MakeCall(nameof(ReadLong), reader);
            }
            else if (type == typeof(decimal))
            {
                return MakeCall(nameof(ReadDecimal), reader);
            }
            else if (type == typeof(float))
            {
                return MakeCall(nameof(ReadFloat), reader);
            }
            else if (type == typeof(double))
            {
                return MakeCall(nameof(ReadDouble), reader);
            }
            else if (type == typeof(bool))
            {
                return MakeCall(nameof(ReadBoolean), reader);
            }
            else if (type == typeof(Guid))
            {
                return MakeCall(nameof(ReadGuid), reader);
            }
            else if (type == typeof(DateTime))
            {
                return MakeCall(nameof(ReadDateTime), reader);
            }
            else if (type == typeof(DateTimeOffset))
            {
                return MakeCall(nameof(ReadDateTimeOffset), reader);
            }
            else if (type == typeof(TimeSpan))
            {
                return MakeCall(nameof(ReadTimeSpan), reader);
            }
            else if (type == typeof(byte?))
            {
                return MakeCall(nameof(ReadNullableByte), reader);
            }
            else if (type == typeof(short?))
            {
                return MakeCall(nameof(ReadNullableShort), reader);
            }
            else if (type == typeof(int?))
            {
                return MakeCall(nameof(ReadNullableInteger), reader);
            }
            else if (type == typeof(long?))
            {
                return MakeCall(nameof(ReadNullableLong), reader);
            }
            else if (type == typeof(decimal?))
            {
                return MakeCall(nameof(ReadNullableDecimal), reader);
            }
            else if (type == typeof(float?))
            {
                return MakeCall(nameof(ReadNullableFloat), reader);
            }
            else if (type == typeof(double?))
            {
                return MakeCall(nameof(ReadNullableDouble), reader);
            }
            else if (type == typeof(bool?))
            {
                return MakeCall(nameof(ReadNullableBoolean), reader);
            }
            else if (type == typeof(Guid?))
            {
                return MakeCall(nameof(ReadNullableGuid), reader);
            }
            else if (type == typeof(DateTime?))
            {
                return MakeCall(nameof(ReadNullableDateTime), reader);
            }
            else if (type == typeof(DateTimeOffset?))
            {
                return MakeCall(nameof(ReadNullableDateTimeOffset), reader);
            }
            else if (type == typeof(TimeSpan?))
            {
                return MakeCall(nameof(ReadNullableTimeSpan), reader);
            }
            else if (type.IsEnum())
            {
                return Expression.Call(typeof(SqlServerJsonValueReader).GetMethod(nameof(ReadEnum)).MakeGenericMethod(type), reader);
            }
            else if (type.UnwrapNullableType().IsEnum())
            {
                return Expression.Call(typeof(SqlServerJsonValueReader).GetMethod(nameof(ReadEnum)).MakeGenericMethod(type.UnwrapNullableType()), reader);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
