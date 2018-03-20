using Impatient.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class KeyEqualityRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly PrimaryKeyDescriptor[] primaryKeyDescriptors;
        private readonly NavigationDescriptor[] navigationDescriptors;

        public KeyEqualityRewritingExpressionVisitor(
            IEnumerable<PrimaryKeyDescriptor> primaryKeyDescriptors,
            IEnumerable<NavigationDescriptor> navigationDescriptors)
        {
            this.primaryKeyDescriptors = primaryKeyDescriptors?.ToArray() ?? throw new ArgumentNullException(nameof(primaryKeyDescriptors));
            this.navigationDescriptors = navigationDescriptors?.ToArray() ?? throw new ArgumentNullException(nameof(navigationDescriptors));
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            // TODO: Test with polymorphism
            if (node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual)
            {
                var left = Visit(node.Left);
                var right = Visit(node.Right);
                var leftIsNullConstant = left is ConstantExpression leftConstant && leftConstant.Value == null;
                var rightIsNullConstant = right is ConstantExpression rightConstant && rightConstant.Value == null;
                var canRewriteLeft = CanRewrite(left);
                var canRewriteRight = CanRewrite(right);

                if ((leftIsNullConstant && rightIsNullConstant)
                    || (!canRewriteLeft && !leftIsNullConstant)
                    || (!canRewriteRight && !rightIsNullConstant))
                {
                    goto Finish;
                }

                left = TryReduceNavigationKey(left, out var rewroteLeft);
                right = TryReduceNavigationKey(right, out var rewroteRight);

                if (rewroteLeft && rewroteRight)
                {
                    goto Finish;
                }

                var primaryKeyDescriptor
                    = primaryKeyDescriptors
                        .FirstOrDefault(d => d.TargetType.IsAssignableFrom(node.Left.Type));

                if (primaryKeyDescriptor == null)
                {
                    goto Finish;
                }

                if (!rewroteLeft && canRewriteLeft)
                {
                    left = primaryKeyDescriptor.KeySelector.ExpandParameters(left);
                }

                if (!rewroteRight && canRewriteRight)
                {
                    right = primaryKeyDescriptor.KeySelector.ExpandParameters(right);
                }

                if (leftIsNullConstant || rightIsNullConstant)
                {
                    var nonNullExpression = leftIsNullConstant ? right : left;

                    if (nonNullExpression is NewExpression newExpression)
                    {
                        nonNullExpression = newExpression.Arguments.First(a => a.Type.IsScalarType());
                    }

                    if (nonNullExpression.Type.GetTypeInfo().IsValueType)
                    {
                        nonNullExpression
                            = Expression.Convert(
                                nonNullExpression,
                                typeof(Nullable<>).MakeGenericType(nonNullExpression.Type));
                    }

                    if (leftIsNullConstant)
                    {
                        left = Expression.Constant(null, nonNullExpression.Type);
                        right = nonNullExpression;
                    }
                    else
                    {
                        left = nonNullExpression;
                        right = Expression.Constant(null, nonNullExpression.Type);
                    }
                }

                Finish:
                return Expression.MakeBinary(node.NodeType, left, right);
            }

            return base.VisitBinary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsQueryableOrEnumerableMethod() && !node.ContainsNonLambdaDelegates())
            {
                switch (node.Method.Name)
                {
                    case nameof(Queryable.GroupJoin):
                    case nameof(Queryable.Join):
                    {
                        var arguments = Visit(node.Arguments).ToArray();
                        var genericArguments = node.Method.GetGenericArguments();
                        var outerKeySelector = arguments[2].UnwrapLambda();
                        var innerKeySelector = arguments[3].UnwrapLambda();

                        if (CanRewrite(outerKeySelector.Body) && CanRewrite(innerKeySelector.Body))
                        {
                            arguments[2] =
                                Expression.Lambda(
                                    TryReduceNavigationKey(outerKeySelector.Body, out var rewroteOuter),
                                    outerKeySelector.Parameters[0]);

                            arguments[3] =
                                Expression.Lambda(
                                    TryReduceNavigationKey(innerKeySelector.Body, out var rewroteInner),
                                    innerKeySelector.Parameters[0]);

                            if (!rewroteOuter || !rewroteInner)
                            {
                                var primaryKeyDescriptor
                                    = primaryKeyDescriptors
                                        .FirstOrDefault(d => d.TargetType.IsAssignableFrom(genericArguments[2]));

                                if (primaryKeyDescriptor != null)
                                {
                                    if (!rewroteOuter)
                                    {
                                        arguments[2]
                                            = Expression.Lambda(
                                                primaryKeyDescriptor.KeySelector.ExpandParameters(outerKeySelector.Body),
                                                outerKeySelector.Parameters[0]);
                                    }

                                    if (!rewroteInner)
                                    {
                                        arguments[3]
                                            = Expression.Lambda(
                                                primaryKeyDescriptor.KeySelector.ExpandParameters(innerKeySelector.Body),
                                                innerKeySelector.Parameters[0]);
                                    }
                                }
                            }

                            if (node.Method.IsQueryableMethod())
                            {
                                arguments[2] = Expression.Quote(arguments[2]);
                                arguments[3] = Expression.Quote(arguments[3]);
                            }

                            genericArguments[2] = arguments[2].UnwrapLambda().ReturnType;

                            return Expression.Call(
                                node.Method.GetGenericMethodDefinition().MakeGenericMethod(genericArguments),
                                arguments);
                        }

                        break;
                    }
                }
            }

            return base.VisitMethodCall(node);
        }

        private bool CanRewrite(Expression expression)
        {
            switch (expression)
            {
                case NewExpression _:
                case MemberInitExpression _:
                case ParameterExpression _:
                {
                    return true;
                }

                default:
                {
                    while (expression is MemberExpression memberExpression)
                    {
                        expression = memberExpression.Expression;
                    }

                    return expression is ParameterExpression;
                }
            }
        }

        private Expression TryReduceNavigationKey(Expression expression, out bool reduced)
        {
            if (expression is MemberExpression memberExpression)
            {
                var navigationDescriptor = navigationDescriptors.FirstOrDefault(n => n.Member == memberExpression.Member);

                if (navigationDescriptor != null)
                {
                    reduced = true;

                    return navigationDescriptor.OuterKeySelector.ExpandParameters(memberExpression.Expression);
                }
            }

            reduced = false;

            return expression;
        }
    }
}
