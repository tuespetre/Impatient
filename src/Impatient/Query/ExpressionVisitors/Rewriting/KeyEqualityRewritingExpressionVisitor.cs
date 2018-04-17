using Impatient.Extensions;
using Impatient.Metadata;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class KeyEqualityRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly DescriptorSet descriptorSet;

        public KeyEqualityRewritingExpressionVisitor(DescriptorSet descriptorSet)
        {
            this.descriptorSet = descriptorSet ?? throw new ArgumentNullException(nameof(descriptorSet));
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            // TODO: Write an explicit test case for PolymorphicExpression key comparison
            if (node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual)
            {
                var left = Visit(node.Left).UnwrapInnerExpression();
                var right = Visit(node.Right).UnwrapInnerExpression();
                var leftIsNullConstant = left is ConstantExpression leftConstant && leftConstant.Value == null;
                var rightIsNullConstant = right is ConstantExpression rightConstant && rightConstant.Value == null;
                var canRewriteLeft = CanRewrite(left);
                var canRewriteRight = CanRewrite(right);

                if ((leftIsNullConstant && rightIsNullConstant)
                    || (!canRewriteLeft && !leftIsNullConstant)
                    || (!canRewriteRight && !rightIsNullConstant))
                {
                    return node;
                }

                left = TryReduceNavigationKey(left, out var rewroteLeft);
                right = TryReduceNavigationKey(right, out var rewroteRight);

                if (rewroteLeft && rewroteRight)
                {
                    return Expression.MakeBinary(node.NodeType, left, right);
                }

                var primaryKeyDescriptor
                    = descriptorSet
                        .PrimaryKeyDescriptors
                        .FirstOrDefault(d => d.TargetType.IsAssignableFrom(node.Left.Type));

                if (primaryKeyDescriptor == null)
                {
                    return Expression.MakeBinary(node.NodeType, left, right);
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
                /*else
                {
                    left = new SqlColumnNullabilityExpressionVisitor().Visit(left);
                    right = new SqlColumnNullabilityExpressionVisitor().Visit(right);
                }*/
                
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

                        if (CanRewrite(outerKeySelector?.Body) && CanRewrite(innerKeySelector?.Body))
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
                                    = descriptorSet
                                        .PrimaryKeyDescriptors
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
                case PolymorphicExpression _:
                //case ExtraPropertiesExpression _:
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
                var navigationDescriptor 
                    = descriptorSet
                        .NavigationDescriptors
                        .FirstOrDefault(n => n.Member == memberExpression.Member);

                if (navigationDescriptor != null)
                {
                    reduced = true;

                    return new SqlColumnNullabilityExpressionVisitor()
                        .Visit(navigationDescriptor.OuterKeySelector.ExpandParameters(memberExpression.Expression));
                }
            }

            reduced = false;

            return expression;
        }
    }
}
