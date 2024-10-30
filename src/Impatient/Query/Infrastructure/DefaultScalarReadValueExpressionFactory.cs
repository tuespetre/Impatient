using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Infrastructure;

public class DefaultScalarReadValueExpressionFactory : IReadValueExpressionFactory
{
    private static readonly MethodInfo readNonNullableMethodInfo
        = typeof(MaterializationUtilities).GetTypeInfo()
            .GetDeclaredMethod(nameof(MaterializationUtilities.ReadNonNullable));

    private static readonly MethodInfo readNullableReferenceMethodInfo
        = typeof(MaterializationUtilities).GetTypeInfo()
            .GetDeclaredMethod(nameof(MaterializationUtilities.ReadNullableReference));

    private static readonly MethodInfo readNullableValueMethodInfo
        = typeof(MaterializationUtilities).GetTypeInfo()
            .GetDeclaredMethod(nameof(MaterializationUtilities.ReadNullableValue));

    private static readonly MethodInfo readNonNullableEnumMethodInfo
        = typeof(MaterializationUtilities).GetTypeInfo()
            .GetDeclaredMethod(nameof(MaterializationUtilities.ReadNonNullableEnum));

    private static readonly MethodInfo readNullableEnumMethodInfo
        = typeof(MaterializationUtilities).GetTypeInfo()
            .GetDeclaredMethod(nameof(MaterializationUtilities.ReadNullableEnum));

    private static readonly MethodInfo readNonNullableConversionMethodInfo
        = typeof(MaterializationUtilities).GetTypeInfo()
            .GetDeclaredMethod(nameof(MaterializationUtilities.ReadNonNullableConversion));

    private static readonly MethodInfo readNullableConversionMethodInfo
        = typeof(MaterializationUtilities).GetTypeInfo()
            .GetDeclaredMethod(nameof(MaterializationUtilities.ReadNullableConversion));

    private readonly ITypeMappingProvider typeMappingProvider;

    public DefaultScalarReadValueExpressionFactory(ITypeMappingProvider typeMappingProvider)
    {
        this.typeMappingProvider = typeMappingProvider;
    }

    public bool CanReadExpression(Expression expression)
    {
        if (expression.Type.IsScalarType())
        {
            return true;
        }

        if (expression is EnumerableRelationalQueryExpression)
        {
            // EF Core changed to always create a type mapping for complex types since they 
            // added JSON support. That's fine for stuff coming from a column, but for enumerable
            // subqueries, it's no bueno
            return false;
        }

        var mapping = FindTypeMapping(expression) ?? typeMappingProvider.FindMapping(expression.Type);

        return mapping is not null;
    }

    public Expression CreateExpression(Expression source, Expression reader, int index)
    {
        var isNullable = (source as SqlColumnExpression)?.IsNullable ?? true;
        var typeMapping = FindTypeMapping(source) ?? typeMappingProvider.FindMapping(source.Type);
        var unwrappedType = source.Type.UnwrapNullableType();
        var arguments = new List<Expression> { reader, Expression.Constant(index) };

        MethodInfo methodInfo;

        if (typeMapping?.TargetConversion is LambdaExpression conversion)
        {
            methodInfo
                = isNullable
                    ? readNullableConversionMethodInfo
                    : readNonNullableConversionMethodInfo;

            methodInfo
                = methodInfo.MakeGenericMethod(
                    conversion.Parameters.Single().Type,
                    source.Type);

            arguments.Add(
                Expression.Lambda(
                    Expression.Convert(conversion.Body, source.Type), 
                    conversion.Parameters));
        }
        else if (unwrappedType.IsEnum())
        {
            methodInfo
                = isNullable
                    ? readNullableEnumMethodInfo.MakeGenericMethod(source.Type, unwrappedType)
                    : readNonNullableEnumMethodInfo.MakeGenericMethod(unwrappedType);
        }
        else
        {
            methodInfo
                = isNullable
                    ? source.Type == unwrappedType
                        ? readNullableReferenceMethodInfo.MakeGenericMethod(source.Type)
                        : readNullableValueMethodInfo.MakeGenericMethod(unwrappedType)
                    : readNonNullableMethodInfo.MakeGenericMethod(source.Type);
        }

        Expression result = Expression.Call(methodInfo, arguments);

        if (result.Type != source.Type)
        {
            result = Expression.Convert(result, source.Type);
        }

        return result;
    }

    private ITypeMapping FindTypeMapping(Expression node)
    {
        switch (node)
        {
            case SqlColumnExpression sqlColumnExpression
            when sqlColumnExpression.TypeMapping is not null:
            {
                return sqlColumnExpression.TypeMapping;
            }

            case SqlAggregateExpression sqlAggregateExpression
            when !sqlAggregateExpression.HasOpaqueType:
            {
                return FindTypeMapping(sqlAggregateExpression.Expression);
            }

            default:
            {
                return typeMappingProvider.FindMapping(node.Type);
            }
        }
    }
}
