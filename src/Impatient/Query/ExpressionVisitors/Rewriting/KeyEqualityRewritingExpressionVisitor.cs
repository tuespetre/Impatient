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
            if (node.NodeType != ExpressionType.Equal && node.NodeType != ExpressionType.NotEqual)
            {
                return base.VisitBinary(node);
            }

            var visitedLeft = Visit(node.Left);
            var visitedRight = Visit(node.Right);

            var left = visitedLeft.UnwrapInnerExpression();
            var right = visitedRight.UnwrapInnerExpression();
            var leftIsNullConstant = left is ConstantExpression leftConstant && leftConstant.Value == null;
            var rightIsNullConstant = right is ConstantExpression rightConstant && rightConstant.Value == null;
            var canRewriteLeft = CanRewrite(left);
            var canRewriteRight = CanRewrite(right);

            if ((leftIsNullConstant && rightIsNullConstant)
                || (!canRewriteLeft && !leftIsNullConstant)
                || (!canRewriteRight && !rightIsNullConstant))
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

            left = TryReduceNavigationKey(left, out var rewroteLeft);
            right = TryReduceNavigationKey(right, out var rewroteRight);

            if (rewroteLeft && rewroteRight)
            {
                return node.UpdateWithConversion(left, right);
            }

            var targetType = leftIsNullConstant ? right.Type : left.Type;

            var primaryKeyDescriptor
                = descriptorSet
                    .PrimaryKeyDescriptors
                    .FirstOrDefault(d => d.TargetType.IsAssignableFrom(targetType));

            if (primaryKeyDescriptor == null)
            {
                return node.Update(visitedLeft, node.Conversion, visitedRight);
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

                switch (nonNullExpression)
                {
                    case NewExpression newExpression:
                    {
                        nonNullExpression
                            = newExpression.Arguments
                                .Select(a => a.UnwrapInnerExpression())
                                .First(a => a.Type.IsScalarType());
                        break;
                    }

                    case NewArrayExpression newArrayExpression:
                    {
                        nonNullExpression
                            = newArrayExpression.Expressions
                                .Select(a => a.UnwrapInnerExpression())
                                .First(a => a.Type.IsScalarType());
                        break;
                    }
                }

                nonNullExpression = nonNullExpression.AsNullable();

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

            return node.UpdateWithConversion(left, right);
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
                            outerKeySelector =
                                Expression.Lambda(
                                    TryReduceNavigationKey(outerKeySelector.Body, out var rewroteOuter),
                                    outerKeySelector.Parameters[0]);

                            innerKeySelector =
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
                                        outerKeySelector
                                            = Expression.Lambda(
                                                primaryKeyDescriptor.KeySelector.ExpandParameters(outerKeySelector.Body),
                                                outerKeySelector.Parameters[0]);
                                    }

                                    if (!rewroteInner)
                                    {
                                        innerKeySelector
                                            = Expression.Lambda(
                                                primaryKeyDescriptor.KeySelector.ExpandParameters(innerKeySelector.Body),
                                                innerKeySelector.Parameters[0]);
                                    }
                                }
                            }
                            
                            if (outerKeySelector.ReturnType != innerKeySelector.ReturnType)
                            {
                                if (outerKeySelector.ReturnType.IsNullableType())
                                {
                                    innerKeySelector
                                        = Expression.Lambda(
                                            Expression.Convert(innerKeySelector.Body, outerKeySelector.ReturnType),
                                            innerKeySelector.Parameters);
                                }
                                else
                                {
                                    outerKeySelector
                                        = Expression.Lambda(
                                            Expression.Convert(outerKeySelector.Body, innerKeySelector.ReturnType),
                                            outerKeySelector.Parameters);
                                }
                            }

                            genericArguments[2] = outerKeySelector.ReturnType;

                            if (node.Method.IsQueryableMethod())
                            {
                                arguments[2] = Expression.Quote(outerKeySelector);
                                arguments[3] = Expression.Quote(innerKeySelector);
                            }
                            else
                            {
                                arguments[2] = outerKeySelector;
                                arguments[3] = innerKeySelector;
                            }

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
                case ExtendedNewExpression _:
                case ExtendedMemberInitExpression _:
                case ParameterExpression _:
                case PolymorphicExpression _:
                case ExtraPropertiesExpression _:
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

                    if (memberExpression.Member.GetMemberType().IsSequenceType())
                    {
                        return new SqlColumnNullabilityExpressionVisitor(true)
                            .Visit(navigationDescriptor.OuterKeySelector.ExpandParameters(memberExpression.Expression));
                    }
                    /*else
                    {
                        return new SqlColumnNullabilityExpressionVisitor(true)
                            .Visit(navigationDescriptor.InnerKeySelector.ExpandParameters(memberExpression));
                    }*/
                }
            }

            reduced = false;

            return expression;
        }
    }
}
