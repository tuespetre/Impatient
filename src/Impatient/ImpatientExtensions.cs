using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Optimizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient
{
    internal static class ImpatientExtensions
    {
        public static Expression ApplyVisitors(this Expression expression, params ExpressionVisitor[] visitors)
        {
            return visitors.Aggregate(expression, (e, v) => v.Visit(e));
        }

        public static Expression ApplyVisitors(this Expression expression, IEnumerable<ExpressionVisitor> visitors)
        {
            return visitors.Aggregate(expression, (e, v) => v.Visit(e));
        }

        public static bool IsComplexNestedQuery(this MethodCallExpression expression)
        {
            return (expression.Method.DeclaringType == typeof(Enumerable)
                && (expression.Method.Name == nameof(Enumerable.ToArray)
                    || expression.Method.Name == nameof(Enumerable.ToList))
                && expression.Arguments[0] is EnumerableRelationalQueryExpression enumerableRelationalQuery
                && enumerableRelationalQuery.SelectExpression.Projection is ServerProjectionExpression);
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

        public static Expression UnwrapAnnotation(this Expression expression)
        {
            return expression is AnnotationExpression annotation ? annotation.Expression : expression;
        }

        public static bool IsTranslatable(this Expression expression)
        {
            return new TranslatabilityAnalyzingExpressionVisitor().Visit(expression) is TranslatableExpression;
        }

        public static bool IsTranslatable(this MemberBinding memberBinding)
        {
            switch (memberBinding)
            {
                case MemberAssignment memberAssignment:
                {
                    return memberAssignment.Expression.IsTranslatable();
                }

                case MemberMemberBinding memberMemberBinding:
                {
                    return memberMemberBinding.Bindings.All(IsTranslatable);
                }

                default:
                {
                    return false;
                }
            }
        }

        public static Expression Replace(this Expression expression, Expression target, Expression replacement)
        {
            return new ExpressionReplacingExpressionVisitor(target, replacement).Visit(expression);
        }

        public static Expression ReduceMemberAccess(this Expression expression)
        {
            return new MemberAccessReducingExpressionVisitor().Visit(expression);
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
            typeof(long),
            typeof(int),
            typeof(short),
            typeof(byte),
            typeof(bool),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(string),
            typeof(byte[]),
            typeof(Guid),
        };
    }
}
