using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors
{
    public class PrimaryKeyDescriptor
    {
        public Type TargetType;
        public LambdaExpression KeySelector;
    }

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

                if (CanRewrite(left) && CanRewrite(right))
                {
                    left = TryReduceNavigationKey(left, out var rewroteLeft);
                    right = TryReduceNavigationKey(right, out var rewroteRight);

                    if (!rewroteLeft || !rewroteRight)
                    {
                        var primaryKeyDescriptor 
                            = primaryKeyDescriptors
                                .FirstOrDefault(d => d.TargetType.IsAssignableFrom(node.Left.Type));

                        if (primaryKeyDescriptor != null)
                        {
                            if (!rewroteLeft)
                            {
                                left = primaryKeyDescriptor.KeySelector.ExpandParameters(left);
                            }

                            if (!rewroteRight)
                            {
                                right = primaryKeyDescriptor.KeySelector.ExpandParameters(right);
                            }
                        }
                    }

                    return Expression.MakeBinary(node.NodeType, left, right);
                }
            }

            return base.VisitBinary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if ((node.Method.DeclaringType == typeof(Queryable) 
                    || node.Method.DeclaringType == typeof(Enumerable))
                && !node.ContainsNonLambdaDelegates())
            {
                switch (node.Method.Name)
                {
                    case nameof(Queryable.GroupBy):
                    {
                        // TODO: Investigate GroupBy support for key equality rewriting (tricky!)
                        break;
                    }

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

                            if (node.Method.DeclaringType == typeof(Queryable))
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
            while (expression is MemberExpression memberExpression)
            {
                expression = memberExpression.Expression;
            }

            // TODO: Support rewriting on the 'element operators' (First, Last, etc.)
            return expression is ParameterExpression;
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
