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

        private static readonly MethodInfo dbDataReaderGetTextReaderMethodInfo
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetTextReader));

        private static readonly MethodInfo dbDataReaderIsDBNullMethodInfo
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

        private static readonly MethodInfo enumerableEmptyMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((object o) => Empty<object>());

        private static readonly MethodInfo enumerableToArrayMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IEnumerable<object> e) => e.ToArray());

        #endregion

        private readonly ITypeMappingProvider typeMappingProvider;

        public SqlServerForJsonReadValueExpressionFactory(ITypeMappingProvider typeMappingProvider)
        {
            this.typeMappingProvider = typeMappingProvider ?? throw new ArgumentNullException(nameof(typeMappingProvider));
        }

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
                = new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReaderVariable, typeMappingProvider)
                    .Visit(source);

            return Expression.Condition(
                Expression.Call(reader, dbDataReaderIsDBNullMethodInfo, Expression.Constant(index)),
                Expression.Convert(CreateDefaultValueExpression(source.Type), source.Type),
                Expression.Call(
                    GetType()
                        .GetMethod(nameof(Materialize), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(source.Type),
                    Expression.Call(
                        reader,
                        dbDataReaderGetTextReaderMethodInfo,
                        Expression.Constant(index)),
                    Expression.Lambda(
                        materializer,
                        "JsonMaterializer",
                        new[] { jsonTextReaderVariable })));
        }

        private static TResult Materialize<TResult>(TextReader textReader, Func<JsonTextReader, TResult> materializer)
        {
            using (var jsonTextReader = new JsonTextReader(textReader))
            {
                jsonTextReader.DateParseHandling = DateParseHandling.None;

                var result = materializer(jsonTextReader);

                return result;
            }
        }

        private static Expression CreateSequenceExpression(Expression expression, Type type)
        {
            var sequenceType = type.GetSequenceType();

            if (type.IsArray)
            {
                return Expression.Call(
                    enumerableToArrayMethodInfo.MakeGenericMethod(sequenceType),
                    expression);
            }

            if (type.IsGenericType(typeof(IQueryable<>)))
            {
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

                case SqlColumnExpression sqlColumnExpression
                when sqlColumnExpression.Table is TableValuedExpressionTableExpression tableValuedExpressionTableExpression:
                {
                    var function = tableValuedExpressionTableExpression.Expression as SqlFunctionExpression;

                    if (function?.FunctionName != "OPENJSON" || function?.FunctionName != "JSON_QUERY")
                    {
                        return node;
                    }

                    if (function.Arguments.Count() == 2)
                    {
                        return node;
                    }

                    return ExtractProjectionExpression(function.Arguments.First());
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
            private readonly ITypeMappingProvider typeMappingProvider;
            private int depth = 0;
            private bool extraProperties;

            public ComplexTypeMaterializerBuildingExpressionVisitor(
                ParameterExpression jsonTextReader,
                ITypeMappingProvider typeMappingProvider)
            {
                this.jsonTextReader = jsonTextReader;
                this.typeMappingProvider = typeMappingProvider;
            }

            private string GetMaterializerName()
            {
                return $"Materialize_{string.Join("_", GetNameParts().DefaultIfEmpty("$root"))}";
            }

            protected override Expression VisitLeaf(Expression node)
            {
                if (node.Type.IsScalarType())
                {
                    var sqlColumnExpression = node as SqlColumnExpression;
                    var typeMapping = sqlColumnExpression?.TypeMapping ?? typeMappingProvider.FindMapping(node.Type);

                    if (typeMapping?.TargetConversion is null)
                    {
                        return SqlServerJsonValueReader.CreateReadScalarExpression(
                            node.Type,
                            jsonTextReader,
                            GetNameParts().Last());
                    }

                    var result 
                        = SqlServerJsonValueReader.CreateReadScalarExpression(
                            typeMapping.SourceType,
                            jsonTextReader,
                            GetNameParts().Last());

                    var mappingParameter = typeMapping.TargetConversion.Parameters.Single();

                    result
                        = Expression.Block(
                            variables: new[] { mappingParameter },
                            expressions: new Expression[]
                            {
                                Expression.Assign(mappingParameter, result),
                                Expression.Convert(typeMapping.TargetConversion.Body, node.Type),
                            });

                    return result;
                }
                else if (node.Type.IsSequenceType())
                {
                    var sequenceType = node.Type.GetSequenceType();
                    var extracted = ExtractProjectionExpression(node);

                    Expression materializer;

                    if (sequenceType.IsScalarType())
                    {
                        // TODO: Get rid of the need to put this constant multiple places.
                        var name = "$c";

                        switch (extracted)
                        {
                            case SqlColumnExpression sqlColumnExpression:
                            {
                                name = sqlColumnExpression.ColumnName;
                                break;
                            }

                            case SqlAliasExpression sqlAliasExpression:
                            {
                                name = sqlAliasExpression.Alias;
                                break;
                            }
                        }

                        materializer = SqlServerJsonValueReader.CreateReadScalarExpression(sequenceType, jsonTextReader, name);
                    }
                    else
                    {
                        if (extracted == node)
                        {
                            return SqlServerJsonValueReader.CreateReadOpaqueObjectExpression(node.Type, jsonTextReader);
                        }

                        var visitor = new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReader, typeMappingProvider);

                        materializer = visitor.Visit(extracted);
                    }

                    return CreateSequenceExpression(
                        SqlServerJsonValueReader.CreateReadArrayExpression(
                            sequenceType,
                            jsonTextReader,
                            GetNameParts().LastOrDefault(),
                            Expression.Lambda(
                                materializer,
                                GetMaterializerName(),
                                Array.Empty<ParameterExpression>())),
                        node.Type);
                }
                else
                {
                    var extracted = ExtractProjectionExpression(node);

                    if (extracted == node)
                    {
                        return SqlServerJsonValueReader.CreateReadOpaqueObjectExpression(node.Type, jsonTextReader);
                    }

                    var visitor = new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReader, typeMappingProvider);
                    var result = visitor.Visit(extracted);

                    if (result is MethodCallExpression call 
                        && call.Method.DeclaringType == typeof(SqlServerJsonValueReader))
                    {
                        return result;
                    }

                    return SqlServerJsonValueReader.CreateReadComplexObjectExpression(
                        node.Type,
                        jsonTextReader,
                        Expression.Lambda(
                            result, 
                            GetMaterializerName(), 
                            Array.Empty<ParameterExpression>()));
                }
            }

            private Expression MaterializeObject(Expression visited)
            {
                if (depth > 0 && !extraProperties)
                {
                    return SqlServerJsonValueReader.CreateReadComplexPropertyExpression(
                        visited.Type,
                        jsonTextReader,
                        GetNameParts().LastOrDefault(),
                        Expression.Lambda(
                            visited, 
                            GetMaterializerName(), 
                            Array.Empty<ParameterExpression>()));
                }

                return visited;
            }

            public override Expression Visit(Expression node)
            {
                switch (node)
                {
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
                    case ExtendedNewExpression _:
                    case ExtendedMemberInitExpression _:
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
                            // We visit the entire ExtraPropertiesExpression below with extraProperties = false
                            // for correctness of the extra property expressions, but that causes the inner row
                            // to be incorrect, so we cache the inner row here so it can be correctly visited
                            // after this block. We should see if there is a way to safely remove the 
                            // extraProperties/depth checks altogether.

                            rowValue = extraPropertiesExpression.Expression;

                            extraProperties = false;

                            extraPropertiesExpression = (ExtraPropertiesExpression)base.Visit(extraPropertiesExpression);

                            var properties = new List<Expression>();

                            for (var i = 0; i < extraPropertiesExpression.Names.Count; i++)
                            {
                                var propertyName = extraPropertiesExpression.Names[i];
                                var propertyValue = extraPropertiesExpression.Properties[i];
                                var propertyVariable = Expression.Variable(propertyValue.Type, propertyName);

                                variables.Add(propertyVariable);
                                properties.Add(propertyVariable);
                                expressions.Add(Expression.Assign(propertyVariable, propertyValue));
                            }
                            
                            rowParameterExpansion = extraPropertiesExpression.Update(rowVariable, properties);
                        }

                        extraProperties = true;

                        rowValue = Visit(rowValue);

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
        public static Expression CreateReadScalarExpression(
            Type type, 
            Expression reader, 
            string name)
        {
            Debug.Assert(reader.Type == typeof(JsonTextReader));

            if (type == typeof(string))
            {
                return MakeCall(nameof(ReadString), reader, name);
            }
            else if (type == typeof(byte[]))
            {
                return MakeCall(nameof(ReadBytes), reader, name);
            }
            else if (type == typeof(byte))
            {
                return MakeCall(nameof(ReadByte), reader, name);
            }
            else if (type == typeof(short))
            {
                return MakeCall(nameof(ReadShort), reader, name);
            }
            else if (type == typeof(int))
            {
                return MakeCall(nameof(ReadInteger), reader, name);
            }
            else if (type == typeof(long))
            {
                return MakeCall(nameof(ReadLong), reader, name);
            }
            else if (type == typeof(decimal))
            {
                return MakeCall(nameof(ReadDecimal), reader, name);
            }
            else if (type == typeof(float))
            {
                return MakeCall(nameof(ReadFloat), reader, name);
            }
            else if (type == typeof(double))
            {
                return MakeCall(nameof(ReadDouble), reader, name);
            }
            else if (type == typeof(bool))
            {
                return MakeCall(nameof(ReadBoolean), reader, name);
            }
            else if (type == typeof(Guid))
            {
                return MakeCall(nameof(ReadGuid), reader, name);
            }
            else if (type == typeof(DateTime))
            {
                return MakeCall(nameof(ReadDateTime), reader, name);
            }
            else if (type == typeof(DateTimeOffset))
            {
                return MakeCall(nameof(ReadDateTimeOffset), reader, name);
            }
            else if (type == typeof(TimeSpan))
            {
                return MakeCall(nameof(ReadTimeSpan), reader, name);
            }
            else if (type == typeof(byte?))
            {
                return MakeCall(nameof(ReadNullableByte), reader, name);
            }
            else if (type == typeof(short?))
            {
                return MakeCall(nameof(ReadNullableShort), reader, name);
            }
            else if (type == typeof(int?))
            {
                return MakeCall(nameof(ReadNullableInteger), reader, name);
            }
            else if (type == typeof(long?))
            {
                return MakeCall(nameof(ReadNullableLong), reader, name);
            }
            else if (type == typeof(decimal?))
            {
                return MakeCall(nameof(ReadNullableDecimal), reader, name);
            }
            else if (type == typeof(float?))
            {
                return MakeCall(nameof(ReadNullableFloat), reader, name);
            }
            else if (type == typeof(double?))
            {
                return MakeCall(nameof(ReadNullableDouble), reader, name);
            }
            else if (type == typeof(bool?))
            {
                return MakeCall(nameof(ReadNullableBoolean), reader, name);
            }
            else if (type == typeof(Guid?))
            {
                return MakeCall(nameof(ReadNullableGuid), reader, name);
            }
            else if (type == typeof(DateTime?))
            {
                return MakeCall(nameof(ReadNullableDateTime), reader, name);
            }
            else if (type == typeof(DateTimeOffset?))
            {
                return MakeCall(nameof(ReadNullableDateTimeOffset), reader, name);
            }
            else if (type == typeof(TimeSpan?))
            {
                return MakeCall(nameof(ReadNullableTimeSpan), reader, name);
            }
            else if (type.IsEnum())
            {
                return Expression.Call(typeof(SqlServerJsonValueReader).GetMethod(nameof(ReadEnum)).MakeGenericMethod(type), reader, Expression.Constant(name));
            }
            else if (type.UnwrapNullableType().IsEnum())
            {
                return Expression.Call(typeof(SqlServerJsonValueReader).GetMethod(nameof(ReadEnum)).MakeGenericMethod(type.UnwrapNullableType()), reader, Expression.Constant(name));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static Expression CreateReadArrayExpression(
            Type elementType, 
            Expression reader, 
            string name, 
            LambdaExpression materializer)
        {
            return Expression.Call(
                typeof(SqlServerJsonValueReader)
                    .GetMethod(nameof(ReadComplexList))
                    .MakeGenericMethod(elementType),
                reader,
                Expression.Constant(name, typeof(string)),
                materializer);
        }

        public static Expression CreateReadComplexPropertyExpression(
            Type type,
            Expression reader,
            string name,
            LambdaExpression materializer)
        {
            return Expression.Call(
                typeof(SqlServerJsonValueReader)
                    .GetMethod(nameof(ReadComplexProperty))
                    .MakeGenericMethod(type),
                reader,
                Expression.Constant(name, typeof(string)),
                materializer);
        }

        public static Expression CreateReadComplexObjectExpression(
            Type type,
            Expression reader,
            LambdaExpression materializer)
        {
            return Expression.Call(
                typeof(SqlServerJsonValueReader)
                    .GetMethod(nameof(ReadComplexObject))
                    .MakeGenericMethod(type),
                reader,
                materializer);
        }

        public static Expression CreateReadOpaqueObjectExpression(
            Type type,
            Expression reader)
        {
            return Expression.Call(
                typeof(SqlServerJsonValueReader)
                    .GetMethod(nameof(ReadOpaqueObject))
                    .MakeGenericMethod(type),
                reader);
        }

        public static Expression MakeCall(string method, Expression reader, string name)
        {
            return Expression.Call(typeof(SqlServerJsonValueReader).GetMethod(method), reader, Expression.Constant(name, typeof(string)));
        }

        public static bool ReadPropertyName(JsonTextReader reader, string name)
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                {
                    break;
                }

                case JsonToken.EndObject:
                case JsonToken.EndArray:
                {
                    return false;
                }

                case JsonToken.Boolean:
                case JsonToken.Bytes:
                case JsonToken.Date:
                case JsonToken.Float:
                case JsonToken.Integer:
                case JsonToken.Null:
                case JsonToken.String:
                {
                    reader.Read();

                    if (reader.TokenType == JsonToken.EndObject ||
                        reader.TokenType == JsonToken.EndArray)
                    {
                        return false;
                    }

                    break;
                }

                default:
                {
                    throw new InvalidOperationException();
                }
            }
            
            Debug.Assert(reader.TokenType == JsonToken.PropertyName);

            return name.Equals(reader.Value);
        }

        public static string ReadString(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                return reader.ReadAsString();
            }

            return default;
        }

        public static byte[] ReadBytes(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                return reader.ReadAsBytes();
            }

            return default;
        }

        public static byte ReadByte(JsonTextReader reader, string name)
        {
            return ReadNullableByte(reader, name).GetValueOrDefault();
        }

        public static byte? ReadNullableByte(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                return (byte?)reader.ReadAsInt32();
            }

            return default;
        }

        public static short ReadShort(JsonTextReader reader, string name)
        {
            return ReadNullableShort(reader, name).GetValueOrDefault();
        }

        public static short? ReadNullableShort(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                return (short?)reader.ReadAsInt32();
            }

            return default;
        }

        public static int ReadInteger(JsonTextReader reader, string name)
        {
            return ReadNullableInteger(reader, name).GetValueOrDefault();
        }

        public static int? ReadNullableInteger(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                return reader.ReadAsInt32();
            }

            return default;
        }

        public static long ReadLong(JsonTextReader reader, string name)
        {
            return ReadNullableLong(reader, name).GetValueOrDefault();
        }

        public static long? ReadNullableLong(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                return (long?)reader.ReadAsDouble();
            }

            return default;
        }

        public static decimal ReadDecimal(JsonTextReader reader, string name)
        {
            return ReadNullableDecimal(reader, name).GetValueOrDefault();
        }

        public static decimal? ReadNullableDecimal(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                return reader.ReadAsDecimal();
            }

            return default;
        }

        public static float ReadFloat(JsonTextReader reader, string name)
        {
            return ReadNullableFloat(reader, name).GetValueOrDefault();
        }

        public static float? ReadNullableFloat(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                return (float?)reader.ReadAsDouble();
            }

            return default;
        }

        public static double ReadDouble(JsonTextReader reader, string name)
        {
            return ReadNullableDouble(reader, name).GetValueOrDefault();
        }

        public static double? ReadNullableDouble(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                return reader.ReadAsDouble();
            }

            return default;
        }

        public static bool ReadBoolean(JsonTextReader reader, string name)
        {
            return ReadNullableBoolean(reader, name).GetValueOrDefault();
        }

        public static bool? ReadNullableBoolean(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                return reader.ReadAsBoolean();
            }

            return default;
        }

        public static Guid ReadGuid(JsonTextReader reader, string name)
        {
            return ReadNullableGuid(reader, name).GetValueOrDefault();
        }

        public static Guid? ReadNullableGuid(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                var value = reader.ReadAsString();

                if (Guid.TryParse(value, out var result))
                {
                    return result;
                }
            }

            return default;
        }

        public static DateTime ReadDateTime(JsonTextReader reader, string name)
        {
            return ReadNullableDateTime(reader, name).GetValueOrDefault();
        }

        public static DateTime? ReadNullableDateTime(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                var value = reader.ReadAsString();

                if (DateTime.TryParse(value, out var result))
                {
                    return result;
                }
            }

            return default;
        }

        public static DateTimeOffset ReadDateTimeOffset(JsonTextReader reader, string name)
        {
            return ReadNullableDateTimeOffset(reader, name).GetValueOrDefault();
        }

        public static DateTimeOffset? ReadNullableDateTimeOffset(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                var value = reader.ReadAsString();

                if (DateTimeOffset.TryParse(value, out var result))
                {
                    return result;
                }
            }

            return default;
        }

        public static TimeSpan ReadTimeSpan(JsonTextReader reader, string name)
        {
            return ReadNullableTimeSpan(reader, name).GetValueOrDefault();
        }

        public static TimeSpan? ReadNullableTimeSpan(JsonTextReader reader, string name)
        {
            if (ReadPropertyName(reader, name))
            {
                var value = reader.ReadAsString();

                if (TimeSpan.TryParse(value, out var result))
                {
                    return result;
                }
            }

            return default;
        }

        public static TEnum ReadEnum<TEnum>(JsonTextReader reader, string name) where TEnum : struct
        {
            return ReadNullableEnum<TEnum>(reader, name).GetValueOrDefault();
        }

        public static TEnum? ReadNullableEnum<TEnum>(JsonTextReader reader, string name) where TEnum : struct
        {
            if (ReadPropertyName(reader, name))
            {
                reader.Read();

                if (reader.Value != null)
                {
                    return (TEnum?)Enum.ToObject(typeof(TEnum), reader.Value);
                }
            }

            return default;
        }

        public static TResult ReadOpaqueObject<TResult>(
            JsonTextReader reader)
        {
            var serializer = new JsonSerializer();

            var result = serializer.Deserialize<TResult>(reader);

            return result;
        }

        public static TResult ReadComplexObject<TResult>(
            JsonTextReader reader, Func<TResult> materializer)
        {
            var result = default(TResult);

            reader.Read();

            Debug.Assert(reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.Null);

            if (reader.TokenType == JsonToken.Null)
            {
                return result;
            }

            result = materializer();

            reader.Read();

            Debug.Assert(reader.TokenType == JsonToken.EndObject);

            return result;
        }

        public static TResult ReadComplexProperty<TResult>(
            JsonTextReader reader, string name, Func<TResult> materializer)
        {
            var result = default(TResult);

            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                {
                    if (!reader.Value.Equals(name))
                    {
                        return result;
                    }

                    reader.Read();

                    if (reader.TokenType == JsonToken.Null)
                    {
                        return result;
                    }

                    break;
                }

                case JsonToken.Boolean:
                case JsonToken.Bytes:
                case JsonToken.Date:
                case JsonToken.Float:
                case JsonToken.Integer:
                case JsonToken.Null:
                case JsonToken.String:
                case JsonToken.EndArray:
                case JsonToken.EndObject:
                {
                    reader.Read();

                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        goto case JsonToken.PropertyName;
                    }

                    return result;
                }

                default:
                {
                    throw new InvalidOperationException();
                }
            }

            Debug.Assert(reader.TokenType == JsonToken.StartObject);

            reader.Read();

            Debug.Assert(reader.TokenType == JsonToken.PropertyName);

            var objectDepth = reader.Depth;

            result = materializer();

            while (reader.Depth > objectDepth)
            {
                reader.Read();

                Debug.Assert(reader.TokenType == JsonToken.EndObject);
            }

            if (reader.Depth == objectDepth)
            {
                reader.Read();

                Debug.Assert(reader.TokenType == JsonToken.EndObject);
            }

            if (reader.Depth == objectDepth - 1 && reader.TokenType == JsonToken.EndObject)
            {
                reader.Read();

                Debug.Assert(
                    reader.TokenType == JsonToken.PropertyName || // next property in parent object
                    reader.TokenType == JsonToken.StartObject || // next object in parent array
                    reader.TokenType == JsonToken.EndObject || // end of parent object
                    reader.TokenType == JsonToken.EndArray); // end of parent array
            }

            return result;
        }

        public static List<TElement> ReadComplexList<TElement>(
            JsonTextReader reader, string name, Func<TElement> materializer)
        {
            var list = new List<TElement>();

            switch (reader.TokenType)
            {
                case JsonToken.None:
                {
                    Debug.Assert(reader.LineNumber == 0 && reader.LinePosition == 0);

                    reader.Read();

                    Debug.Assert(reader.TokenType == JsonToken.StartArray);

                    break;
                }

                case JsonToken.StartArray:
                {
                    //Debug.Assert(name == null);

                    break;
                }

                case JsonToken.PropertyName:
                {
                    if (name != null && !reader.Value.Equals(name))
                    {
                        return list;
                    }

                    reader.Read();

                    if (reader.TokenType == JsonToken.Null)
                    {
                        return list;
                    }

                    Debug.Assert(reader.TokenType == JsonToken.StartArray);

                    break;
                }

                case JsonToken.Boolean:
                case JsonToken.Bytes:
                case JsonToken.Date:
                case JsonToken.Float:
                case JsonToken.Integer:
                case JsonToken.Null:
                case JsonToken.String:
                case JsonToken.EndArray:
                case JsonToken.EndObject:
                {
                    reader.Read();

                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        goto case JsonToken.PropertyName;
                    }

                    return list;
                }

                default:
                {
                    throw new InvalidOperationException();
                }
            }

            var arrayDepth = reader.Depth + 1;

            ReadElementOrEnd:

            switch (reader.TokenType)
            {
                case JsonToken.EndArray:
                {
                    goto EndArray;
                }

                case JsonToken.StartArray:
                case JsonToken.EndObject:
                {
                    reader.Read();

                    if (reader.TokenType == JsonToken.EndArray)
                    {
                        goto EndArray;
                    }

                    break;
                }
            }

            ReadElement:

            Debug.Assert(reader.TokenType == JsonToken.StartObject);

            reader.Read();

            Debug.Assert(reader.TokenType == JsonToken.PropertyName);

            var element = materializer();

            list.Add(element);

            if (reader.TokenType == JsonToken.StartObject)
            {
                goto ReadElement;
            }

            while (reader.Depth > arrayDepth)
            {
                reader.Read();

                Debug.Assert(reader.TokenType == JsonToken.EndObject);
            }

            goto ReadElementOrEnd;

            EndArray:

            reader.Read();

            Debug.Assert(
                reader.TokenType == JsonToken.PropertyName || // next property in parent object
                reader.TokenType == JsonToken.StartArray || // next array in parent array
                reader.TokenType == JsonToken.EndObject || // end of parent object
                reader.TokenType == JsonToken.EndArray || // end of parent array
                reader.TokenType == JsonToken.None); // end of json

            return list;
        }
    }
}
