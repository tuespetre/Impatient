using Impatient.Query;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient
{
    public static class ImpatientExtensions
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
    }
}
