using Impatient.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class NullOrDefaultEqualityOptimizingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            var leftConstant = left.UnwrapInnerExpression() as ConstantExpression;
            var rightConstant = right.UnwrapInnerExpression() as ConstantExpression;
            var leftDefault = left.UnwrapInnerExpression() as DefaultExpression;
            var rightDefault = right.UnwrapInnerExpression() as DefaultExpression;

            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                {
                    if (leftConstant != null && rightConstant != null)
                    {
                        if (leftConstant.Value is null && rightConstant.Value is null)
                        {
                            return Expression.Constant(true);
                        }
                        else if (leftConstant.Value is null || rightConstant.Value is null)
                        {
                            return Expression.Constant(false);
                        }
                    }
                    else if (leftDefault != null && rightDefault != null)
                    {
                        if (HaveSameNullability(leftDefault, rightDefault))
                        {
                            return Expression.Constant(true);
                        }
                        else
                        {
                            return Expression.Constant(false);
                        }
                    }
                    else
                    {
                        var constantOperand = leftConstant ?? rightConstant;
                        var defaultOperand = leftDefault ?? rightDefault;
                        var otherOperand = (leftConstant == null && leftDefault == null) ? left : right;

                        if (constantOperand != null && defaultOperand != null)
                        {
                            if (constantOperand.Value is null)
                            {
                                if (IsNullable(defaultOperand))
                                {
                                    return Expression.Constant(true);
                                }
                                else
                                {
                                    return Expression.Constant(false);
                                }
                            }
                            else
                            {
                                if (IsNullable(defaultOperand))
                                {
                                    return Expression.Constant(false);
                                }
                                else
                                {
                                    return Expression.Constant(
                                        constantOperand.Value.Equals(
                                            Activator.CreateInstance(defaultOperand.Type)));
                                }
                            }
                        }
                        else if ((constantOperand != null && constantOperand.Value is null)
                            || (defaultOperand != null && IsNullable(defaultOperand)))
                        {
                            switch (otherOperand.UnwrapInnerExpression())
                            {
                                case NewExpression newExpression:
                                {
                                    return Expression.Constant(NewExpressionIsNull(newExpression));
                                }

                                case MemberInitExpression memberInitExpression:
                                {
                                    return Expression.Constant(NewExpressionIsNull(memberInitExpression.NewExpression));
                                }

                                case NewArrayExpression _:
                                case ListInitExpression _:
                                case Expression expression when !IsNullable(expression):
                                {
                                    return Expression.Constant(false);
                                }
                            }
                        }
                        else if (otherOperand.Type.GetTypeInfo().IsValueType
                            && (constantOperand != null && constantOperand.Value.Equals(Activator.CreateInstance(otherOperand.Type)))
                                || (defaultOperand != null && !IsNullable(defaultOperand)))
                        {
                            switch (otherOperand.UnwrapInnerExpression())
                            {
                                case NewExpression newExpression:
                                {
                                    return Expression.Constant(NewExpressionIsDefault(newExpression));
                                }

                                case MemberInitExpression memberInitExpression:
                                {
                                    return Expression.Constant(NewExpressionIsDefault(memberInitExpression.NewExpression));
                                }

                                case NewArrayExpression _:
                                case ListInitExpression _:
                                {
                                    return Expression.Constant(false);
                                }
                            }
                        }
                    }

                    break;
                }

                case ExpressionType.NotEqual:
                {
                    if (leftConstant != null && rightConstant != null)
                    {
                        if (leftConstant.Value is null && rightConstant.Value is null)
                        {
                            return Expression.Constant(false);
                        }
                        else if (leftConstant.Value is null || rightConstant.Value is null)
                        {
                            return Expression.Constant(true);
                        }
                    }
                    else if (leftDefault != null && rightDefault != null)
                    {
                        if (HaveSameNullability(leftDefault, rightDefault))
                        {
                            return Expression.Constant(false);
                        }
                        else
                        {
                            return Expression.Constant(true);
                        }
                    }
                    else
                    {
                        var constantOperand = leftConstant ?? rightConstant;
                        var defaultOperand = leftDefault ?? rightDefault;
                        var otherOperand = (leftConstant == null && leftDefault == null) ? left : right;

                        if (constantOperand != null && defaultOperand != null)
                        {
                            if (constantOperand.Value is null)
                            {
                                if (IsNullable(defaultOperand))
                                {
                                    return Expression.Constant(false);
                                }
                                else
                                {
                                    return Expression.Constant(true);
                                }
                            }
                            else
                            {
                                if (IsNullable(defaultOperand))
                                {
                                    return Expression.Constant(true);
                                }
                                else
                                {
                                    return Expression.Constant(
                                        !constantOperand.Value.Equals(
                                            Activator.CreateInstance(defaultOperand.Type)));
                                }
                            }
                        }
                        else if ((constantOperand != null && constantOperand.Value is null)
                            || (defaultOperand != null && IsNullable(defaultOperand)))
                        {
                            switch (otherOperand.UnwrapInnerExpression())
                            {
                                case NewExpression newExpression:
                                {
                                    return Expression.Constant(!NewExpressionIsNull(newExpression));
                                }

                                case MemberInitExpression memberInitExpression:
                                {
                                    return Expression.Constant(!NewExpressionIsNull(memberInitExpression.NewExpression));
                                }

                                case NewArrayExpression _:
                                case ListInitExpression _:
                                case Expression expression when !IsNullable(expression):
                                {
                                    return Expression.Constant(true);
                                }
                            }
                        }
                        else if (otherOperand.Type.GetTypeInfo().IsValueType
                            && (constantOperand != null && constantOperand.Value.Equals(Activator.CreateInstance(otherOperand.Type)))
                                || (defaultOperand != null && !IsNullable(defaultOperand)))
                        {
                            switch (otherOperand.UnwrapInnerExpression())
                            {
                                case NewExpression newExpression:
                                {
                                    return Expression.Constant(!NewExpressionIsDefault(newExpression));
                                }

                                case MemberInitExpression memberInitExpression:
                                {
                                    return Expression.Constant(!NewExpressionIsDefault(memberInitExpression.NewExpression));
                                }

                                case NewArrayExpression _:
                                case ListInitExpression _:
                                {
                                    return Expression.Constant(true);
                                }
                            }
                        }
                    }

                    break;
                }
            }

            return node.Update(left, node.Conversion, right);
        }

        private static bool HaveSameNullability(Expression left, Expression right)
        {
            var leftNullable = IsNullable(left);
            var rightNullable = IsNullable(right);

            return (leftNullable && rightNullable) || (!leftNullable && !rightNullable);
        }

        private static bool IsNullable(Expression node)
        {
            return node.Type.IsNullableType() || !node.Type.GetTypeInfo().IsValueType;
        }

        private static bool NewExpressionIsNull(NewExpression newExpression)
        {
            if (newExpression.Type.IsNullableType())
            {
                return newExpression.Arguments.Count == 0;
            }

            return false;
        }

        private static bool NewExpressionIsDefault(NewExpression newExpression)
        {
            if (newExpression.Type.GetTypeInfo().IsValueType)
            {
                return newExpression.Arguments.Count == 0;
            }

            return false;
        }
    }
}
