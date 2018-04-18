using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Extensions
{
    public static class ExpressionExtensions
    {
        public static Expression AsEnumerableQuery(this Expression expression)
        {
            Debug.Assert(expression.Type.IsSequenceType());

            var sequenceType = expression.Type.GetSequenceType();

            return Expression.New(
                typeof(EnumerableQuery<>).MakeGenericType(sequenceType).GetConstructor(new[] { typeof(Expression) }),
                Expression.Call(
                    typeof(Expression).GetRuntimeMethod(nameof(Expression.Constant), new[] { typeof(object) }),
                    expression));
        }

        private static Expression ResolveProperty(Expression expression, string segment)
        {
            switch (expression)
            {
                case NewExpression newExpression:
                {
                    var match = newExpression.Members?.FirstOrDefault(m => m.GetPathSegmentName() == segment);

                    if (match != null)
                    {
                        return newExpression.Arguments[newExpression.Members.IndexOf(match)];
                    }

                    return null;
                }

                case MemberInitExpression memberInitExpression:
                {
                    var match = ResolveProperty(memberInitExpression.NewExpression, segment);

                    if (match != null)
                    {
                        return match;
                    }

                    match
                        = memberInitExpression.Bindings
                            .OfType<MemberAssignment>()
                            .Where(a => a.Member.GetPathSegmentName() == segment)
                            .Select(a => a.Expression)
                            .FirstOrDefault();

                    if (match != null)
                    {
                        return match;
                    }

                    return null;
                }

                case ExtraPropertiesExpression extraPropertiesExpression:
                {
                    for (var i = 0; i < extraPropertiesExpression.Names.Count; i++)
                    {
                        var name = extraPropertiesExpression.Names[i];

                        if (name.Equals(segment))
                        {
                            return extraPropertiesExpression.Properties[i];
                        }
                    }

                    return ResolveProperty(extraPropertiesExpression.Expression, segment);
                }

                case AnnotationExpression annotationExpression:
                {
                    return ResolveProperty(annotationExpression.Expression, segment);
                }

                case PolymorphicExpression polymorphicExpression:
                {
                    foreach (var descriptor in polymorphicExpression.Descriptors)
                    {
                        var expanded = descriptor.Materializer.ExpandParameters(polymorphicExpression.Row);

                        var resolved = ResolveProperty(expanded, segment);

                        if (resolved != null)
                        {
                            return resolved;
                        }
                    }

                    return null;
                }

                default:
                {
                    return null;
                }
            }
        }

        public static bool TryResolvePath(this Expression expression, string path, out Expression resolved)
        {
            resolved = expression;

            foreach (var segment in path.Split('.'))
            {
                var next = ResolveProperty(resolved, segment);

                if (next == null)
                {
                    resolved = null;
                    return false;
                }

                resolved = next;
            }

            return true;
        }
        
        public static Expression ReplaceWithConversions(this Expression expression, Func<Expression, Expression> replacer)
        {
            var conversionStack = new Stack<UnaryExpression>();

            while (expression is UnaryExpression unaryExpression
                && expression.NodeType == ExpressionType.Convert)
            {
                conversionStack.Push(unaryExpression);

                expression = unaryExpression.Operand;
            }

            expression = replacer(expression);

            while (conversionStack.Count != 0)
            {
                expression = conversionStack.Pop().Update(expression);
            }

            return expression;
        }

        public static Expression AsSqlBooleanExpression(this Expression expression)
        {
            var unwrapped = expression.UnwrapInnerExpression();

            if (unwrapped.IsSqlBooleanExpression())
            {
                return unwrapped;
            }

            var test = true;

            while (expression.NodeType == ExpressionType.Not)
            {
                test = !test;
                expression = ((UnaryExpression)expression).Operand;
            }

            return Expression.Equal(expression, Expression.Constant(test));
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
                // TODO: Include SqlLikeExpression
                {
                    return true;
                }

                case BinaryExpression _:
                {
                    switch (expression.NodeType)
                    {
                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                        case ExpressionType.AndAlso:
                        case ExpressionType.OrElse:
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                        case ExpressionType.And when expression.Type.IsBooleanType():
                        case ExpressionType.Or when expression.Type.IsBooleanType():
                        //case ExpressionType.Coalesce when expression.Type.IsBooleanType():
                        {
                            return true;
                        }

                        default:
                        {
                            return false;
                        }
                    }
                }

                case UnaryExpression unaryExpression:
                {
                    switch (expression.NodeType)
                    {
                        case ExpressionType.Not:
                        {
                            // TODO: Include SqlLikeExpression
                            return unaryExpression.Operand is SqlInExpression
                                || unaryExpression.Operand is SqlExistsExpression;
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

        private static readonly MethodInfo enumerableToListMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IEnumerable<object> o) => o.ToList());

        public static Expression AsCollectionType(this Expression sequence)
        {
            if (!sequence.Type.IsGenericType(typeof(ICollection<>)))
            {
                sequence
                    = Expression.Call(
                        enumerableToListMethodInfo.MakeGenericMethod(sequence.Type.GetSequenceType()),
                        sequence);
            }

            return sequence;
        }

        public static bool ContainsNonLambdaDelegates(this MethodCallExpression methodCallExpression)
        {
            return methodCallExpression.Arguments
                .Where(a => typeof(Delegate).IsAssignableFrom(a.Type))
                .Any(a => a.NodeType != ExpressionType.Lambda);
        }

        public static bool ContainsNonLambdaExpressions(this MethodCallExpression methodCallExpression)
        {
            return methodCallExpression.Arguments
                .Where(a => typeof(Expression).IsAssignableFrom(a.Type))
                .Any(a => a.NodeType != ExpressionType.Quote);
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

        /// <summary>
        /// Removes any wrapping <see cref="AnnotationExpression"/>s, <see cref="ExtraPropertiesExpression"/>s,
        /// and <see cref="UnaryExpression"/>s with type <see cref="ExpressionType.Convert"/> and returns the
        /// inner expression.
        /// </summary>
        public static Expression UnwrapInnerExpression(this Expression expression)
        {
            if (expression is AnnotationExpression annotationExpression)
            {
                return annotationExpression.Expression.UnwrapInnerExpression();
            }
            else if (expression is ExtraPropertiesExpression extraPropertiesExpression)
            {
                return extraPropertiesExpression.Expression.UnwrapInnerExpression();
            }
            else if (expression.NodeType == ExpressionType.Convert)
            {
                return ((UnaryExpression)expression).Operand.UnwrapInnerExpression();
            }
            else
            {
                return expression;
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
    }
}
