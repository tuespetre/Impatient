using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient
{
    internal static class ImpatientExtensions
    {
        #region Naive implementations of Enumerable operators from .NET Core 2.0

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            yield return element;
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            yield return element;

            foreach (var item in source)
            {
                yield return item;
            }
        }

        public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            return source.Reverse().Skip(count).Reverse();
        }

        public static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            return source.Reverse().Take(count).Reverse();
        }

        #endregion

        public static Expression AsSqlBooleanExpression(this Expression expression)
        {
            expression = expression.UnwrapAnnotations();

            if (expression == null || expression.IsSqlBooleanExpression())
            {
                return expression;
            }

            return Expression.Equal(expression, Expression.Constant(true));
        }

        public static bool IsSqlBooleanExpression(this Expression expression)
        {
            switch (expression)
            {
                case null:
                {
                    return false;
                }

                case SqlExistsExpression _:
                case SqlInExpression _:
                // TODO: Include SqlLikeExpression case here
                {
                    return true;
                }

                case BinaryExpression _:
                {
                    switch (expression.NodeType)
                    {
                        case ExpressionType.Constant:
                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                        case ExpressionType.AndAlso:
                        case ExpressionType.OrElse:
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                        {
                            return true;
                        }

                        default:
                        {
                            return false;
                        }
                    }
                }

                default:
                {
                    return false;
                }
            }
        }

        public static IEnumerable<MemberBinding> Iterate(this IEnumerable<MemberBinding> bindings)
        {
            foreach (var binding in bindings)
            {
                switch (binding)
                {
                    case MemberAssignment memberAssignment:
                    {
                        yield return memberAssignment;

                        break;
                    }

                    case MemberListBinding memberListBinding:
                    {
                        yield return memberListBinding;

                        break;
                    }

                    case MemberMemberBinding memberMemberBinding:
                    {
                        foreach (var yielded in memberMemberBinding.Bindings.Iterate())
                        {
                            yield return yielded;
                        }

                        break;
                    }
                }
            }
        }

        public static bool IsQueryableMethod(this MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(Queryable);
        }

        public static bool IsEnumerableMethod(this MethodInfo methodInfo)
        {
            return methodInfo.DeclaringType == typeof(Enumerable);
        }

        public static bool IsQueryableOrEnumerableMethod(this MethodInfo methodInfo)
        {
            return methodInfo.IsQueryableMethod() || methodInfo.IsEnumerableMethod();
        }

        public static bool IsOrderingMethod(this MethodInfo methodInfo)
        {
            return methodInfo.IsQueryableOrEnumerableMethod()
                && (methodInfo.Name == nameof(Queryable.OrderBy)
                    || methodInfo.Name == nameof(Queryable.OrderByDescending)
                    || methodInfo.Name == nameof(Queryable.ThenBy)
                    || methodInfo.Name == nameof(Queryable.ThenByDescending));
        }

        public static bool HasComparerArgument(this MethodInfo methodInfo)
        {
            return methodInfo
                .GetParameters()
                .Select(p => p.ParameterType)
                .Any(t => t.IsGenericType(typeof(IEqualityComparer<>))
                    || t.IsGenericType(typeof(IComparer<>)));
        }

        public static bool ContainsNonLambdaDelegates(this MethodCallExpression methodCallExpression)
        {
            return methodCallExpression.Arguments
                .Where(a => typeof(Delegate).IsAssignableFrom(a.Type))
                .Any(a => a.NodeType != ExpressionType.Lambda);
        }

        public static BinaryExpression Balance(this BinaryExpression binaryExpression)
        {
            return BinaryBalancingExpressionVisitor.Instance.VisitAndConvert(binaryExpression, nameof(Balance));
        }

        public static IEnumerable<Expression> SplitNodes(this Expression expression, ExpressionType splitOn)
        {
            switch (expression)
            {
                case BinaryExpression binaryExpression
                when binaryExpression.NodeType == splitOn:
                {
                    foreach (var left in SplitNodes(binaryExpression.Left, splitOn))
                    {
                        yield return left;
                    }

                    foreach (var right in SplitNodes(binaryExpression.Right, splitOn))
                    {
                        yield return right;
                    }

                    yield break;
                }

                default:
                {
                    yield return expression;
                    yield break;
                }
            }
        }

        public static Expression VisitWith(this Expression expression, IEnumerable<ExpressionVisitor> visitors)
        {
            return visitors.Aggregate(expression, (e, v) => v.Visit(e));
        }

        public static MethodInfo GetGenericMethodDefinition<TArg, TResult>(Expression<Func<TArg, TResult>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method.GetGenericMethodDefinition();
        }

        public static LambdaExpression UnwrapLambda(this Expression expression)
        {
            switch (expression?.NodeType)
            {
                case ExpressionType.Quote:
                {
                    return ((UnaryExpression)expression).Operand as LambdaExpression;
                }

                default:
                {
                    return expression as LambdaExpression;
                }
            }
        }

        public static Expression UnwrapAnnotations(this Expression expression)
        {
            while (expression is AnnotationExpression annotationExpression)
            {
                expression = annotationExpression.Expression;
            }

            return expression;
        }

        public static Expression Replace(this Expression expression, Expression target, Expression replacement)
        {
            return new ExpressionReplacingExpressionVisitor(target, replacement).Visit(expression);
        }

        public static Expression ExpandParameters(this LambdaExpression lambdaExpression, params Expression[] expansions)
        {
            var lambdaBody = lambdaExpression.Body;

            for (var i = 0; i < expansions.Length; i++)
            {
                lambdaBody = lambdaBody.Replace(lambdaExpression.Parameters[i], expansions[i]);
            }

            return new MemberAccessReducingExpressionVisitor().Visit(lambdaBody);
        }

        public static bool References(this Expression expression, Expression targetExpression)
        {
            var referenceCountingVisitor = new ReferenceCountingExpressionVisitor(targetExpression);

            referenceCountingVisitor.Visit(expression);

            return referenceCountingVisitor.ReferenceCount > 0;
        }

        public static string GetPathSegmentName(this MemberInfo memberInfo)
        {
            return memberInfo.IsDefined(typeof(PathSegmentNameAttribute))
                ? memberInfo.GetCustomAttribute<PathSegmentNameAttribute>().Name
                : memberInfo.Name;
        }

        public static bool IsNullableType(this Type type)
        {
            return type != null
                && type.IsConstructedGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type MakeNullableType(this Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                return typeof(Nullable<>).MakeGenericType(type);
            }

            return type;
        }

        public static bool IsBooleanType(this Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        public static bool IsSequenceType(this Type type)
        {
            return type.IsGenericType(typeof(IEnumerable<>));
        }

        public static Type GetSequenceType(this Type type)
        {
            return type.FindGenericType(typeof(IEnumerable<>))?.GenericTypeArguments[0];
        }

        public static bool IsGenericType(this Type type, Type definition)
        {
            return type.FindGenericType(definition) != null;
        }

        public static Type FindGenericType(this Type type, Type definition)
        {
            if (type == null || type == typeof(object))
            {
                return null;
            }
            else if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == definition)
            {
                return type;
            }

            var definitionTypeInfo = definition.GetTypeInfo();

            if (definitionTypeInfo.IsInterface)
            {
                foreach (var interfaceType in type.GetInterfaces())
                {
                    if (interfaceType.IsConstructedGenericType && interfaceType.GetGenericTypeDefinition() == definition)
                    {
                        return interfaceType;
                    }
                }

                return null;
            }
            else
            {
                GetBaseType:

                var baseType = type.GetTypeInfo().BaseType;

                if (baseType == null || baseType == typeof(object))
                {
                    return null;
                }
                else if (baseType.IsConstructedGenericType && baseType.GetGenericTypeDefinition() == definition)
                {
                    return baseType;
                }
                else
                {
                    goto GetBaseType;
                }
            }
        }

        public static bool IsScalarType(this Type type)
        {
            if (scalarTypes.Contains(type))
            {
                return true;
            }

            if (type.IsNullableType())
            {
                var underlyingType = Nullable.GetUnderlyingType(type);

                return scalarTypes.Contains(underlyingType);
            }

            return false;
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                {
                    return propertyInfo.PropertyType;
                }

                case FieldInfo fieldInfo:
                {
                    return fieldInfo.FieldType;
                }

                default:
                {
                    return typeof(void);
                }
            }
        }

        private static readonly Type[] scalarTypes =
        {
            // Value types
            typeof(long), // bigint
            typeof(int), // int
            typeof(short), // smallint
            typeof(byte), // tinyint
            typeof(bool), // bit
            typeof(decimal), // decimal, money, numeric, smallmoney
            typeof(double), // float
            typeof(float), // real
            typeof(TimeSpan), // time
            typeof(DateTime), // datetime2, datetime, date, smalldatetime
            typeof(DateTimeOffset), // datetimeoffset
            typeof(Guid), // uniqueidentifer

            // Reference types
            typeof(string), // nvarchar, varchar, nchar, char, ntext, text
            typeof(byte[]), // varbinary, binary, FILESTREAM, image, rowversion, timestamp

            // Unmapped types
            // - sql_variant
            // - xml

            // Supported enum types (for future reference):
            // - byte
            // - short
            // - int
            // - long
            // - string (explicitly)
        };

        public static bool IsConstantLiteralType(this Type type)
        {
            return Array.IndexOf(constantLiteralTypes, type) > -1;
        }

        private static readonly Type[] constantLiteralTypes =
        {
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(char),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(string),
        };

        public static MethodInfo MatchQueryableMethod(MethodInfo method)
        {
            if (method.IsEnumerableMethod())
            {
                return method;
            }

            var genericMethodDefinition = method.GetGenericMethodDefinition();

            var genericArguments = genericMethodDefinition.GetGenericArguments();

            var parameterTypes
                = genericMethodDefinition.GetParameters()
                    .Select(p =>
                    {
                        if (p.ParameterType.IsConstructedGenericType)
                        {
                            var genericTypeDefinition = p.ParameterType.GetGenericTypeDefinition();

                            if (genericTypeDefinition == typeof(Expression<>))
                            {
                                return p.ParameterType.GenericTypeArguments[0];
                            }
                            else if (genericTypeDefinition == typeof(IQueryable<>))
                            {
                                return typeof(IEnumerable<>).MakeGenericType(p.ParameterType.GenericTypeArguments[0]);
                            }
                            else if (genericTypeDefinition == typeof(IOrderedQueryable<>))
                            {
                                return typeof(IOrderedEnumerable<>).MakeGenericType(p.ParameterType.GenericTypeArguments[0]);
                            }
                            else
                            {
                                return p.ParameterType;
                            }
                        }
                        else if (p.ParameterType == typeof(IQueryable))
                        {
                            return typeof(IEnumerable);
                        }
                        else
                        {
                            return p.ParameterType;
                        }
                    })
                    .ToArray();

            bool TypesMatch(Type type1, Type type2)
            {
                if (type1 == type2)
                {
                    return true;
                }
                else if (type1.IsConstructedGenericType && type2.IsConstructedGenericType)
                {
                    var genericType1 = type1.GetGenericTypeDefinition();
                    var genericType2 = type2.GetGenericTypeDefinition();

                    return genericType1 == genericType2
                        && type1.GenericTypeArguments.Zip(type2.GenericTypeArguments, TypesMatch).All(b => b);
                }
                else if (type1.IsGenericParameter && type2.IsGenericParameter)
                {
                    return type1.Name == type2.Name
                        && type1.GenericParameterPosition == type2.GenericParameterPosition;
                }
                else
                {
                    return false;
                }
            }

            var matching = (from m in enumerableMethods

                            where m.Name == method.Name

                            let parameters = m.GetParameters()
                            where parameters.Length == parameterTypes.Length
                            where m.GetParameters().Select(p => p.ParameterType).Zip(parameterTypes, TypesMatch).All(b => b)

                            let arguments = m.GetGenericArguments()
                            where arguments.Length == genericArguments.Length
                            where arguments.Zip(genericArguments, TypesMatch).All(b => b)

                            select m).ToList();
            
            return matching.Single().MakeGenericMethod(method.GetGenericArguments());
        }

        private static readonly IEnumerable<MethodInfo> enumerableMethods
            = typeof(Enumerable).GetTypeInfo().DeclaredMethods;
    }
}
