using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient
{
    internal static class ImpatientExtensions
    {
        public static bool IsBoolean(this Type type)
        {
            return type == typeof(bool) || type == typeof(bool?);
        }

        public static Expression VisitWith(this Expression expression, IEnumerable<ExpressionVisitor> visitors)
        {
            return visitors.Aggregate(expression, (e, v) => v.Visit(e));
        }

        public static bool MatchesGenericMethod(this MethodInfo method, MethodInfo other)
        {
            return method.IsGenericMethod && method.GetGenericMethodDefinition() == other;
        }
        
        public static MethodInfo GetMethodDefinition<TArg, TResult>(Expression<Func<TArg, TResult>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }

        public static MethodInfo GetGenericMethodDefinition<TArg, TResult>(Expression<Func<TArg, TResult>> expression)
        {
            return GetMethodDefinition(expression).GetGenericMethodDefinition();
        }

        public static bool IsAssignableFrom(this Type type, Type otherType)
        {
            return type.GetTypeInfo().IsAssignableFrom(otherType.GetTypeInfo());
        }

        public static Type GetSequenceType(this Type type)
        {
            return type.FindGenericType(typeof(IEnumerable<>))?.GenericTypeArguments[0];
        }

        public static Type FindGenericType(this Type type, Type definition)
        {
            var definitionTypeInfo = definition.GetTypeInfo();

            while (type != null && type != typeof(object))
            {
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == definition)
                {
                    return type;
                }

                var typeTypeInfo = type.GetTypeInfo();

                if (definitionTypeInfo.IsInterface)
                {
                    foreach (var interfaceType in typeTypeInfo.ImplementedInterfaces)
                    {
                        var found = interfaceType.FindGenericType(definition);

                        if (found != null)
                        {
                            return found;
                        }
                    }
                }

                type = type.GetTypeInfo().BaseType;
            }

            return null;
        }

        public static LambdaExpression UnwrapLambda(this Expression expression)
        {
            switch (expression.NodeType)
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

        public static bool IsScalarType(this Type type)
        {
            if (scalarTypes.Contains(type))
            {
                return true;
            }

            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType)
            {
                var genericTypeDefinition = typeInfo.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    var underlyingType = typeInfo.GenericTypeArguments[0];

                    return scalarTypes.Contains(underlyingType);
                }
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
