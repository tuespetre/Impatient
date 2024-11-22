using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Projection;
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

namespace Impatient.Query.Infrastructure;

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
                    [jsonTextReaderVariable])));
    }

    private static TResult Materialize<TResult>(TextReader textReader, Func<JsonTextReader, TResult> materializer)
    {
        using var jsonTextReader = new JsonTextReader(textReader);

        jsonTextReader.DateParseHandling = DateParseHandling.None;

        var result = materializer(jsonTextReader);

        return result;
    }

    private static Expression CreateSequenceExpression(Expression expression, Type type)
    {
        if (!(type.IsArray || type.IsQueryableType()))
        {
            return expression;
        }

        var result = expression;
        var sequenceType = type.GetSequenceType();
        var source = Expression.Parameter(expression.Type, "source");

        if (type.IsArray)
        {
            result = source.AsArray();
        }
        else if (type.IsQueryableType())
        {
            // Calling AsQueryable creates a self-referencing
            // EnumerableQuery whose inner list/array/etc. cannot
            // be accessed without reflection. We want other visitors
            // to have the chance to access it so we manually construct
            // the EnumerableQuery from a ConstantExpression.
            result = source.AsEnumerableQuery();

            if (type.IsOrderedQueryableType())
            {
                result
                    = Expression.New(
                        typeof(StubOrderedQueryableEnumerable<>)
                            .MakeGenericType(type.GetSequenceType())
                            .GetTypeInfo()
                            .DeclaredConstructors
                            .Single(),
                        result);
            }
        }

        return Expression.Block(
            variables: [source],
            expressions:
            [
                Expression.Assign(source, expression),
                Expression.Condition(
                    Expression.Equal(Expression.Constant(null), source),
                    Expression.Constant(null, result.Type),
                    result)
            ]);
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
                var extracted = ExtractProjectionExpression(sqlAliasExpression.Expression);

                if (extracted != sqlAliasExpression.Expression)
                {
                    return extracted;
                }
                else
                {
                    return sqlAliasExpression;
                }
            }

            case SqlColumnExpression sqlColumnExpression
            when sqlColumnExpression.Table is TableValuedExpressionTableExpression tableValuedExpressionTableExpression:
            {
                var function = tableValuedExpressionTableExpression.Expression as SqlFunctionExpression;

                if (function?.FunctionName is not ("OPENJSON" or "JSON_QUERY"))
                {
                    return node;
                }

                if (function.Arguments.Count > 1)
                {
                    return node;
                }

                var query = function.Arguments[0];

                var extracted = ExtractProjectionExpression(query);

                if (extracted.Type == node.Type)
                {
                    return extracted;
                }
                else
                {
                    return node;
                }
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
            when methodCallExpression.Method.Name == nameof(Queryable.FirstOrDefault): // TODO: ummmm what about others, like LastOrDefault?
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

        private ComplexTypeMaterializerBuildingExpressionVisitor(
            ParameterExpression jsonTextReader,
            ITypeMappingProvider typeMappingProvider,
            int depth) : this(jsonTextReader, typeMappingProvider)
        {
            this.depth = depth;
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
                        variables: [mappingParameter],
                        expressions:
                        [
                            Expression.Assign(mappingParameter, result),
                            Expression.Convert(typeMapping.TargetConversion.Body, node.Type),
                        ]);

                return result;
            }
            else if (node.Type.IsSequenceType())
            {
                var sequenceType = node.Type.GetSequenceType();

                if (sequenceType.IsScalarType())
                {
                    var extracted = ExtractProjectionExpression(node);
                    var name = GetLeafName(extracted);

                    var materializer
                        = SqlServerJsonValueReader.CreateReadScalarExpression(
                            sequenceType,
                            jsonTextReader,
                            name);

                    if (name is not null)
                    {
                        materializer
                            = SqlServerJsonValueReader.CreateReadComplexAsElementExpression(
                                materializer.Type,
                                jsonTextReader,
                                Expression.Lambda(
                                    materializer,
                                    GetMaterializerName(),
                                    []));
                    }

                    return MaterializeList(node, materializer);
                }
                else
                {
                    var extracted = ExtractProjectionExpression(node);
                    var name = GetLeafName(extracted);

                    if (extracted == node)
                    {
                        return MaterializeOpaque(node);
                    }

                    var visitor = new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReader, typeMappingProvider, depth + 1);
                    var materializer = visitor.Visit(extracted);

                    if (name is not null)
                    {
                        materializer
                            = SqlServerJsonValueReader.CreateReadComplexAsElementExpression(
                                materializer.Type,
                                jsonTextReader,
                                Expression.Lambda(
                                    materializer,
                                    GetMaterializerName(),
                                    []));
                    }

                    return MaterializeList(node, materializer);
                }
            }
            else
            {
                var extracted = ExtractProjectionExpression(node);

                if (extracted == node)
                {
                    return MaterializeOpaque(node);
                }

                var visitor = new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReader, typeMappingProvider, depth + 1);

                var result = visitor.Visit(extracted);

                if (result is MethodCallExpression call && call.Method.DeclaringType == typeof(SqlServerJsonValueReader))
                {
                    return result;
                }

                return MaterializeComplex(node);
            }
        }

        private string GetLeafName(Expression node)
        {
            switch (node)
            {
                case SqlColumnExpression sqlColumnExpression:
                {
                    return sqlColumnExpression.ColumnName;
                }

                case SqlAliasExpression sqlAliasExpression:
                {
                    return sqlAliasExpression.Alias;
                }

                default:
                {
                    return null;
                }
            }
        }

        private Expression MaterializeList(Expression node, Expression materializer)
        {
            if (depth > 0)
            {
                var name = GetNameParts().LastOrDefault() ?? GetLeafName(node);

                if (name is null)
                {
                    return CreateSequenceExpression(
                        SqlServerJsonValueReader.CreateReadListAsElementExpression(
                            node.Type.GetSequenceType(),
                            jsonTextReader,
                            Expression.Lambda(
                                materializer,
                                GetMaterializerName(),
                                [])),
                        node.Type);
                }
                else
                {
                    return CreateSequenceExpression(
                        SqlServerJsonValueReader.CreateReadListAsPropertyExpression(
                            node.Type.GetSequenceType(),
                            jsonTextReader,
                            name,
                            Expression.Lambda(
                                materializer,
                                GetMaterializerName(),
                                [])),
                        node.Type);
                }
            }
            else
            {
                return CreateSequenceExpression(
                    SqlServerJsonValueReader.CreateReadListAsRootExpression(
                        node.Type.GetSequenceType(),
                        jsonTextReader,
                        Expression.Lambda(
                            materializer,
                            GetMaterializerName(),
                            [])),
                    node.Type);
            }
        }

        private Expression MaterializeOpaque(Expression node)
        {
            if (depth > 0)
            {
                var name = GetNameParts().LastOrDefault() ?? GetLeafName(node);

                if (name is null)
                {
                    return SqlServerJsonValueReader.CreateReadOpaqueAsElementExpression(
                        node.Type,
                        jsonTextReader);
                }
                else
                {
                    return SqlServerJsonValueReader.CreateReadOpaqueAsPropertyExpression(
                        node.Type,
                        jsonTextReader,
                        name);
                }
            }
            else
            {
                return SqlServerJsonValueReader.CreateReadOpaqueAsRootExpression(
                    node.Type,
                    jsonTextReader);
            }
        }

        private Expression MaterializeComplex(Expression materializer)
        {
            if (extraProperties)
            {
                return materializer;
            }
            else if (depth > 0)
            {
                var name = GetNameParts().LastOrDefault();

                if (name is null)
                {
                    return SqlServerJsonValueReader.CreateReadComplexAsElementExpression(
                        materializer.Type,
                        jsonTextReader,
                        Expression.Lambda(
                            materializer,
                            GetMaterializerName(),
                            []));
                }
                else
                {
                    return SqlServerJsonValueReader.CreateReadComplexAsPropertyExpression(
                        materializer.Type,
                        jsonTextReader,
                        name,
                        Expression.Lambda(
                            materializer,
                            GetMaterializerName(),
                            []));
                }
            }
            else
            {
                return SqlServerJsonValueReader.CreateReadComplexAsRootExpression(
                    materializer.Type,
                    jsonTextReader,
                    Expression.Lambda(
                        materializer,
                        GetMaterializerName(),
                        []));
            }
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case DefaultIfEmptyExpression defaultIfEmpty:
                {
                    return Visit(defaultIfEmpty.Expression);
                }

                case ExtraPropertiesExpression:
                {
                    var flag = extraProperties;

                    extraProperties = true;

                    var visited = base.Visit(node);

                    extraProperties = flag;

                    return MaterializeComplex(visited);
                }

                case NewExpression newExpression when IsNotLeaf(newExpression):
                case MemberInitExpression memberInitExpression when IsNotLeaf(memberInitExpression):
                case ExtendedNewExpression:
                case ExtendedMemberInitExpression:
                {
                    var flag = extraProperties;

                    depth++;

                    extraProperties = false;

                    var visited = base.Visit(node);

                    extraProperties = flag;

                    depth--;

                    return MaterializeComplex(visited);
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
                        // to be incorrect, so we cache the inner row here so it can be correctly materializer
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

                    return MaterializeComplex(Expression.Block(variables, expressions));
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
    private static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        ObjectCreationHandling = ObjectCreationHandling.Replace,
    };

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
            if (name is null)
            {
                return Expression.Call(
                    typeof(SqlServerJsonValueReader).GetMethod(nameof(ReadEnum), [typeof(JsonTextReader)]).MakeGenericMethod(type), 
                    reader);
            }
            else
            {
                return Expression.Call(
                    typeof(SqlServerJsonValueReader).GetMethod(nameof(ReadEnum), [typeof(JsonTextReader), typeof(string)]).MakeGenericMethod(type),
                    reader,
                    Expression.Constant(name));
            }
        }
        else if (type.UnwrapNullableType().IsEnum())
        {
            if (name is null)
            {
                return Expression.Call(
                    typeof(SqlServerJsonValueReader).GetMethod(nameof(ReadNullableEnum), [typeof(JsonTextReader)]).MakeGenericMethod(type.UnwrapNullableType()),
                    reader);
            }
            else
            {
                return Expression.Call(
                    typeof(SqlServerJsonValueReader).GetMethod(nameof(ReadNullableEnum), [typeof(JsonTextReader), typeof(string)]).MakeGenericMethod(type.UnwrapNullableType()),
                    reader,
                    Expression.Constant(name));
            }
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    private static MethodCallExpression MakeCall(string method, Expression reader, string name)
    {
        if (name is null)
        {
            return Expression.Call(
                typeof(SqlServerJsonValueReader).GetMethod(method, [typeof(JsonTextReader)]),
                reader);
        }
        else
        {
            return Expression.Call(
                typeof(SqlServerJsonValueReader).GetMethod(method, [typeof(JsonTextReader), typeof(string)]),
                reader,
                Expression.Constant(name, typeof(string)));
        }
    }

    public static Expression CreateReadListAsRootExpression(
        Type elementType,
        Expression reader,
        LambdaExpression materializer)
    {
        return Expression.Call(
            typeof(SqlServerJsonValueReader)
                .GetMethod(nameof(ReadListAsRoot))
                .MakeGenericMethod(elementType),
            reader,
            materializer);
    }

    public static Expression CreateReadListAsElementExpression(
        Type elementType,
        Expression reader,
        LambdaExpression materializer)
    {
        return Expression.Call(
            typeof(SqlServerJsonValueReader)
                .GetMethod(nameof(ReadListAsElement))
                .MakeGenericMethod(elementType),
            reader,
            materializer);
    }

    public static Expression CreateReadListAsPropertyExpression(
        Type elementType,
        Expression reader,
        string name,
        LambdaExpression materializer)
    {
        ArgumentNullException.ThrowIfNull(name);

        return Expression.Call(
            typeof(SqlServerJsonValueReader)
                .GetMethod(nameof(ReadListAsProperty))
                .MakeGenericMethod(elementType),
            reader,
            Expression.Constant(name, typeof(string)),
            materializer);
    }

    public static Expression CreateReadComplexAsRootExpression(
        Type type,
        Expression reader,
        LambdaExpression materializer)
    {
        return Expression.Call(
            typeof(SqlServerJsonValueReader)
                .GetMethod(nameof(ReadComplexAsRoot))
                .MakeGenericMethod(type),
            reader,
            materializer);
    }

    public static Expression CreateReadComplexAsElementExpression(
        Type type,
        Expression reader,
        LambdaExpression materializer)
    {
        return Expression.Call(
            typeof(SqlServerJsonValueReader)
                .GetMethod(nameof(ReadComplexAsElement))
                .MakeGenericMethod(type),
            reader,
            materializer);
    }

    public static Expression CreateReadComplexAsPropertyExpression(
        Type type,
        Expression reader,
        string name,
        LambdaExpression materializer)
    {
        ArgumentNullException.ThrowIfNull(name);

        return Expression.Call(
            typeof(SqlServerJsonValueReader)
                .GetMethod(nameof(ReadComplexAsProperty))
                .MakeGenericMethod(type),
            reader,
            Expression.Constant(name, typeof(string)),
            materializer);
    }

    public static Expression CreateReadOpaqueAsRootExpression(
        Type type,
        Expression reader)
    {
        return Expression.Call(
            typeof(SqlServerJsonValueReader)
                .GetMethod(nameof(ReadOpaqueAsRoot))
                .MakeGenericMethod(type),
            reader);
    }

    public static Expression CreateReadOpaqueAsElementExpression(
        Type type,
        Expression reader)
    {
        return Expression.Call(
            typeof(SqlServerJsonValueReader)
                .GetMethod(nameof(ReadOpaqueAsElement))
                .MakeGenericMethod(type),
            reader);
    }

    public static Expression CreateReadOpaqueAsPropertyExpression(
        Type type,
        Expression reader,
        string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return Expression.Call(
            typeof(SqlServerJsonValueReader)
                .GetMethod(nameof(ReadOpaqueAsProperty))
                .MakeGenericMethod(type),
            reader,
            Expression.Constant(name, typeof(string)));
    }

    private static bool ReadPropertyName(JsonTextReader reader, string name)
    {
        if (reader.TokenType is JsonToken.PropertyName)
        {
            if (name.Equals(reader.Value))
            {
                reader.Read();

                return true;
            }
            else
            {
                return false;
            }
        }
        else if (reader.TokenType is JsonToken.EndObject)
        {
            return false;
        }
        else
        {
            var message = "Issue reading JSON property name";
            Debug.Fail(message);
            throw new InvalidOperationException(message);
        }
    }

    // string

    public static string ReadString(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadString(reader);
        }
        else
        {
            return default;
        }
    }

    public static string ReadString(JsonTextReader reader)
    {
        var result = reader.Value?.ToString();

        reader.Read();

        return result;
    }

    // byte[]

    public static byte[] ReadBytes(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadBytes(reader);
        }
        else
        {
            return default;
        }
    }

    public static byte[] ReadBytes(JsonTextReader reader)
    {
        byte[] result = reader.Value switch
        {
            null => null,
            string x => Convert.FromBase64String(x),
        };

        reader.Read();

        return result;
    }

    // byte

    public static byte ReadByte(JsonTextReader reader, string name)
    {
        return ReadNullableByte(reader, name).GetValueOrDefault();
    }

    public static byte? ReadNullableByte(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableByte(reader);
        }
        else
        {
            return default;
        }
    }

    public static byte ReadByte(JsonTextReader reader)
    {
        return ReadNullableByte(reader).GetValueOrDefault();
    }

    public static byte? ReadNullableByte(JsonTextReader reader)
    {
        byte? result = reader.Value switch
        {
            null => null,
            long x => (byte?)x,
        };

        reader.Read();

        return result;
    }

    // short

    public static short ReadShort(JsonTextReader reader, string name)
    {
        return ReadNullableShort(reader, name).GetValueOrDefault();
    }

    public static short? ReadNullableShort(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableShort(reader);
        }
        else
        {
            return default;
        }
    }

    public static short ReadShort(JsonTextReader reader)
    {
        return ReadNullableShort(reader).GetValueOrDefault();
    }

    public static short? ReadNullableShort(JsonTextReader reader)
    {
        short? result = reader.Value switch
        {
            null => null,
            long x => (short?)x,
        };

        reader.Read();

        return result;
    }

    // int

    public static int ReadInteger(JsonTextReader reader, string name)
    {
        return ReadNullableInteger(reader, name).GetValueOrDefault();
    }

    public static int? ReadNullableInteger(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableInteger(reader);
        }
        else
        {
            return default;
        }
    }

    public static int ReadInteger(JsonTextReader reader)
    {
        return ReadNullableInteger(reader).GetValueOrDefault();
    }

    public static int? ReadNullableInteger(JsonTextReader reader)
    {
        int? result = reader.Value switch
        {
            null => null,
            int x => x,
            long x => (int)x,
            string x => int.Parse(x),
        };

        reader.Read();

        return result;
    }

    // long

    public static long ReadLong(JsonTextReader reader, string name)
    {
        return ReadNullableLong(reader, name).GetValueOrDefault();
    }

    public static long? ReadNullableLong(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableLong(reader);
        }
        else
        {
            return default;
        }
    }

    public static long ReadLong(JsonTextReader reader)
    {
        return ReadNullableLong(reader).GetValueOrDefault();
    }

    public static long? ReadNullableLong(JsonTextReader reader)
    {
        var result = (long?)reader.Value;

        reader.Read();

        return result;
    }

    // decimal

    public static decimal ReadDecimal(JsonTextReader reader, string name)
    {
        return ReadNullableDecimal(reader, name).GetValueOrDefault();
    }

    public static decimal? ReadNullableDecimal(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableDecimal(reader);
        }
        else
        {
            return default;
        }
    }

    public static decimal ReadDecimal(JsonTextReader reader)
    {
        return ReadNullableDecimal(reader).GetValueOrDefault();
    }

    public static decimal? ReadNullableDecimal(JsonTextReader reader)
    {
        decimal? result = reader.Value switch
        {
            null => null,
            double x => (decimal?)x,
        };

        reader.Read();

        return result;
    }

    // float

    public static float ReadFloat(JsonTextReader reader, string name)
    {
        return ReadNullableFloat(reader, name).GetValueOrDefault();
    }

    public static float? ReadNullableFloat(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableFloat(reader);
        }
        else
        {
            return default;
        }
    }

    public static float ReadFloat(JsonTextReader reader)
    {
        return ReadNullableFloat(reader).GetValueOrDefault();
    }

    public static float? ReadNullableFloat(JsonTextReader reader)
    {
        float? result = reader.Value switch
        {
            null => null,
            double x => (float?)x,
        };

        reader.Read();

        return result;
    }

    // double

    public static double ReadDouble(JsonTextReader reader, string name)
    {
        return ReadNullableDouble(reader, name).GetValueOrDefault();
    }

    public static double? ReadNullableDouble(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableDouble(reader);
        }
        else
        {
            return default;
        }
    }

    public static double ReadDouble(JsonTextReader reader)
    {
        return ReadNullableDouble(reader).GetValueOrDefault();
    }

    public static double? ReadNullableDouble(JsonTextReader reader)
    {
        var result = (double?)reader.Value;

        reader.Read();

        return result;
    }

    // bool

    public static bool ReadBoolean(JsonTextReader reader, string name)
    {
        return ReadNullableBoolean(reader, name).GetValueOrDefault();
    }

    public static bool? ReadNullableBoolean(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableBoolean(reader);
        }
        else
        {
            return default;
        }
    }

    public static bool ReadBoolean(JsonTextReader reader)
    {
        return ReadNullableBoolean(reader).GetValueOrDefault();
    }

    public static bool? ReadNullableBoolean(JsonTextReader reader)
    {
        bool? result = reader.Value switch
        {
            null => null,
            bool x => x,
            0L => false,
            1L => true,
        };

        reader.Read();

        return result;
    }

    // Guid

    public static Guid ReadGuid(JsonTextReader reader, string name)
    {
        return ReadNullableGuid(reader, name).GetValueOrDefault();
    }

    public static Guid? ReadNullableGuid(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableGuid(reader);
        }
        else
        {
            return default;
        }
    }

    public static Guid ReadGuid(JsonTextReader reader)
    {
        return ReadNullableGuid(reader).GetValueOrDefault();
    }

    public static Guid? ReadNullableGuid(JsonTextReader reader)
    {
        var value = (string)reader.Value;

        reader.Read();

        if (Guid.TryParse(value, out var result))
        {
            return result;
        }

        return default;
    }

    // DateTime

    public static DateTime ReadDateTime(JsonTextReader reader, string name)
    {
        return ReadNullableDateTime(reader, name).GetValueOrDefault();
    }

    public static DateTime? ReadNullableDateTime(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableDateTime(reader);
        }
        else
        {
            return default;
        }
    }

    public static DateTime ReadDateTime(JsonTextReader reader)
    {
        return ReadNullableDateTime(reader).GetValueOrDefault();
    }

    public static DateTime? ReadNullableDateTime(JsonTextReader reader)
    {
        var value = (string)reader.Value;

        reader.Read();

        if (DateTime.TryParse(value, out var result))
        {
            return result;
        }

        return default;
    }

    // DateTimeOffset

    public static DateTimeOffset ReadDateTimeOffset(JsonTextReader reader, string name)
    {
        return ReadNullableDateTimeOffset(reader, name).GetValueOrDefault();
    }

    public static DateTimeOffset? ReadNullableDateTimeOffset(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableDateTimeOffset(reader);
        }
        else
        {
            return default;
        }
    }

    public static DateTimeOffset ReadDateTimeOffset(JsonTextReader reader)
    {
        return ReadNullableDateTimeOffset(reader).GetValueOrDefault();
    }

    public static DateTimeOffset? ReadNullableDateTimeOffset(JsonTextReader reader)
    {
        var value = (string)reader.Value;

        reader.Read();

        if (DateTimeOffset.TryParse(value, out var result))
        {
            return result;
        }

        return default;
    }

    // TimeSpan

    public static TimeSpan ReadTimeSpan(JsonTextReader reader, string name)
    {
        return ReadNullableTimeSpan(reader, name).GetValueOrDefault();
    }

    public static TimeSpan? ReadNullableTimeSpan(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableTimeSpan(reader);
        }
        else
        {
            return default;
        }
    }

    public static TimeSpan ReadTimeSpan(JsonTextReader reader)
    {
        return ReadNullableTimeSpan(reader).GetValueOrDefault();
    }

    public static TimeSpan? ReadNullableTimeSpan(JsonTextReader reader)
    {
        var value = (string)reader.Value;

        reader.Read();

        if (TimeSpan.TryParse(value, out var result))
        {
            return result;
        }

        return default;
    }

    // enum

    public static TEnum ReadEnum<TEnum>(JsonTextReader reader, string name) where TEnum : struct
    {
        return ReadNullableEnum<TEnum>(reader, name).GetValueOrDefault();
    }

    public static TEnum? ReadNullableEnum<TEnum>(JsonTextReader reader, string name) where TEnum : struct
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadNullableEnum<TEnum>(reader);
        }
        else
        {
            return default;
        }
    }

    public static TEnum ReadEnum<TEnum>(JsonTextReader reader) where TEnum : struct
    {
        return ReadNullableEnum<TEnum>(reader).GetValueOrDefault();
    }

    public static TEnum? ReadNullableEnum<TEnum>(JsonTextReader reader) where TEnum : struct
    {
        var result = default(TEnum?);

        if (reader.Value is not null)
        {
            result = (TEnum?)Enum.ToObject(typeof(TEnum), reader.Value);
        }

        reader.Read();

        return result;
    }

    // opaque

    public static TResult ReadOpaqueAsRoot<TResult>(JsonTextReader reader)
    {
        Debug.Assert(reader.TokenType is JsonToken.None);

        var result = JsonSerializer.Create(jsonSerializerSettings).Deserialize<TResult>(reader);

        return result;
    }

    public static TResult ReadOpaqueAsElement<TResult>(JsonTextReader reader)
    {
        if (reader.TokenType is JsonToken.Null)
        {
            return default;
        }

        TResult result;

        if (reader.TokenType is JsonToken.String)
        {
            result = JsonConvert.DeserializeObject<TResult>((string)reader.Value, jsonSerializerSettings);
        }
        else
        {
            result = JsonSerializer.Create(jsonSerializerSettings).Deserialize<TResult>(reader);
        }

        reader.Read();

        return result;
    }

    public static TResult ReadOpaqueAsProperty<TResult>(JsonTextReader reader, string name)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadOpaqueAsElement<TResult>(reader);
        }
        else
        {
            return default;
        }
    }

    // complex

    public static TResult ReadComplexAsRoot<TResult>(JsonTextReader reader, Func<TResult> materializer)
    {
        Debug.Assert(reader.TokenType is JsonToken.None);

        reader.Read();

        return ReadComplexAsElement(reader, materializer);
    }

    public static TResult ReadComplexAsElement<TResult>(JsonTextReader reader, Func<TResult> materializer)
    {
        if (reader.TokenType is JsonToken.Null)
        {
            reader.Read();

            return default;
        }
        else
        {
            Debug.Assert(reader.TokenType is JsonToken.StartObject);

            reader.Read();

            var result = materializer();

            Debug.Assert(reader.TokenType == JsonToken.EndObject);

            reader.Read();

            return result;
        }
    }

    public static TResult ReadComplexAsProperty<TResult>(JsonTextReader reader, string name, Func<TResult> materializer)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadComplexAsElement(reader, materializer);
        }
        else
        {
            return default;
        }
    }

    // list

    public static List<TElement> ReadListAsRoot<TElement>(JsonTextReader reader, Func<TElement> materializer)
    {
        Debug.Assert(reader.TokenType is JsonToken.None);

        reader.Read();

        return ReadListAsElement(reader, materializer);
    }

    public static List<TElement> ReadListAsElement<TElement>(JsonTextReader reader, Func<TElement> materializer)
    {
        if (reader.TokenType is JsonToken.Null)
        {
            reader.Read();

            return [];
        }
        else
        {
            Debug.Assert(reader.TokenType is JsonToken.StartArray);

            reader.Read();

            var result = new List<TElement>();

            while (reader.TokenType is not JsonToken.EndArray)
            {
                var element = materializer();

                result.Add(element);
            }

            reader.Read();

            return result;
        }
    }

    public static List<TElement> ReadListAsProperty<TElement>(JsonTextReader reader, string name, Func<TElement> materializer)
    {
        if (ReadPropertyName(reader, name))
        {
            return ReadListAsElement(reader, materializer);
        }
        else
        {
            return [];
        }
    }
}
