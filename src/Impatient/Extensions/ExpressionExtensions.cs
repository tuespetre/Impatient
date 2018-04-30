using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
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
        public static bool IsSemanticallyEqualTo(this Expression expression, Expression other)
        {
            return ExpressionEqualityComparer.Instance.GetHashCode(expression)
                == ExpressionEqualityComparer.Instance.GetHashCode(other);
        }

        public static bool IsNullConstant(this Expression expression)
        {
            switch (expression.UnwrapInnerExpression())
            {
                case ConstantExpression constantExpression:
                {
                    return constantExpression.Value == null;
                }

                case DefaultExpression defaultExpression:
                {
                    return defaultExpression.Type.IsNullableType()
                        || !defaultExpression.Type.GetTypeInfo().IsValueType;
                }

                default:
                {
                    return false;
                }
            }
        }

        public static Expression AsNullable(this Expression expression)
        {
            return Expression.Convert(expression, expression.Type.MakeNullableType());
        }

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

        public static void MatchNullableTypes(ref Expression left, ref Expression right)
        {
            if (left.Type == right.Type)
            {
                return;
            }
            else if (left.Type.UnwrapNullableType() == right.Type)
            {
                right = Expression.Convert(right, left.Type);
            }
            else if (right.Type.UnwrapNullableType() == left.Type)
            {
                left = Expression.Convert(left, right.Type);
            }
        }

        public static BinaryExpression Update(this BinaryExpression node, Expression left, Expression right)
        {
            if (left.Type != node.Left.Type)
            {
                left = Expression.Convert(left, node.Left.Type);
            }

            if (right.Type != node.Right.Type)
            {
                right = Expression.Convert(right, node.Right.Type);
            }

            return node.Update(left, node.Conversion, right);
        }

        /// <summary>
        /// Ensures that the given expression is a valid expression
        /// for producing a predicatable boolean value or wraps it otherwise.
        /// Examples: IN, EXISTS, x = y, a &lt; b
        /// </summary>
        public static Expression AsLogicalBooleanSqlExpression(this Expression expression)
        {
            if (expression.IsLogicalBooleanSqlExpression())
            {
                return expression;
            }

            var test = true;

            expression = expression.UnwrapInnerExpression();

            while (expression.NodeType == ExpressionType.Not)
            {
                test = !test;
                expression = ((UnaryExpression)expression).Operand;
            }

            var testType = expression.Type.IsBooleanType() ? expression.Type : typeof(bool);

            return Expression.Equal(expression, Expression.Constant(test, testType));
        }

        /// <summary>
        /// Ensures that the given expression is a valid expression
        /// for producing a projected boolean value or wraps it otherwise.
        /// Examples: [table].[Column], CASE WHEN x = y THEN 1 ELSE 0 END
        /// </summary>
        public static Expression AsBooleanValuedSqlExpression(this Expression expression)
        {
            if (expression.IsBooleanValuedSqlExpression())
            {
                return expression;
            }

            expression = expression.UnwrapInnerExpression();

            var flag = true;
            var flipped = false;
            var nullable = expression.Type.IsNullableType();

            while (expression.NodeType == ExpressionType.Not)
            {
                flag = !flag;
                flipped = true;
                expression = ((UnaryExpression)expression).Operand;
            }

            if (expression.IsBooleanValuedSqlExpression())
            {
                if (nullable)
                {
                    return Expression.Condition(
                        Expression.Equal(expression, Expression.Constant(null)),
                        Expression.Constant(null, typeof(bool?)),
                        flipped
                            ? Expression.Convert(
                                Expression.Equal(
                                    expression, 
                                    Expression.Constant(flag, typeof(bool?))),
                                typeof(bool?))
                            : expression);
                }
                else
                {
                    return Expression.Condition(
                        Expression.Equal(expression, Expression.Constant(flag)),
                        Expression.Constant(true),
                        Expression.Constant(false));
                }
            }

            var test = expression;

            if (!test.IsLogicalBooleanSqlExpression())
            {
                if (nullable)
                {
                    test
                        = Expression.AndAlso(
                            Expression.NotEqual(test, Expression.Constant(null)),
                            test.AsLogicalBooleanSqlExpression());
                }
                else
                {
                    test = test.AsLogicalBooleanSqlExpression();
                }
            }

            var ifTrue = (Expression)Expression.Constant(flag);
            var ifFalse = (Expression)Expression.Constant(!flag);

            if (nullable)
            {
                if (test.NodeType == ExpressionType.Convert)
                {
                    test = ((UnaryExpression)test).Operand;
                }
                else
                {
                    ifTrue = Expression.Convert(ifTrue, typeof(bool?));
                    ifFalse = Expression.Constant(null, typeof(bool?));
                }
            }

            return Expression.Condition(test, ifTrue, ifFalse);
        }

        public static bool IsLogicalBooleanSqlExpression(this Expression expression)
        {
            if (!expression.Type.IsBooleanType())
            {
                return false;
            }

            var unwrapped = expression.UnwrapInnerExpression();

            switch (unwrapped)
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
                    switch (unwrapped.NodeType)
                    {
                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                        case ExpressionType.AndAlso:
                        case ExpressionType.OrElse:
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                        case ExpressionType.And:
                        case ExpressionType.Or:
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
                    switch (unwrapped.NodeType)
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

        public static bool IsBooleanValuedSqlExpression(this Expression expression)
        {
            if (!expression.Type.IsBooleanType())
            {
                return false;
            }

            var unwrapped = expression.UnwrapInnerExpression();

            switch (unwrapped)
            {
                case SqlInExpression _:
                case SqlExistsExpression _:
                {
                    return false;
                }

                case BinaryExpression _ when unwrapped.NodeType == ExpressionType.Coalesce:
                case ConditionalExpression _:
                case ConstantExpression _:
                case SqlExpression _:
                case RelationalQueryExpression _:
                {
                    return true;
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
