using Impatient.Extensions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class ConditionalComparisonOptimizingExpressionVisitor : ExpressionVisitor
    {
        private BooleanOptimizingExpressionVisitor booleanOptimizingExpressionVisitor
            = new BooleanOptimizingExpressionVisitor();

        private NullOrDefaultEqualityOptimizingExpressionVisitor nullEqualityOptimizingExpressionVisitor
            = new NullOrDefaultEqualityOptimizingExpressionVisitor();

        private Expression OptimizeAndRevisit(Expression node)
        {
            node = nullEqualityOptimizingExpressionVisitor.Visit(node);
            node = booleanOptimizingExpressionVisitor.Visit(node);

            return Visit(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            var test = Visit(node.Test);

            if (test.UnwrapInnerExpression() is ConstantExpression constant)
            {
                return true.Equals(constant.Value) ? Visit(node.IfTrue) : Visit(node.IfFalse);
            }

            var ifTrue = Visit(node.IfTrue);
            var ifFalse = Visit(node.IfFalse);

            if (ifTrue.IsSemanticallyEqualTo(ifFalse))
            {
                return ifTrue;
            }

            return node.Update(test, ifTrue, ifFalse);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var visitedLeft = Visit(node.Left);
            var visitedRight = Visit(node.Right);

            if (node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual)
            {
                var leftConditional = visitedLeft as ConditionalExpression;
                var rightConditional = visitedRight as ConditionalExpression;

                if (leftConditional != null && rightConditional != null)
                {
                    var leftTest = leftConditional.Test;
                    var leftIfTrue = leftConditional.IfTrue;
                    var leftIfFalse = leftConditional.IfFalse;
                    var rightTest = rightConditional.Test;
                    var rightIfTrue = rightConditional.IfTrue;
                    var rightIfFalse = rightConditional.IfFalse;

                    // (a ? b : c) @ (d ? e : f)

                    if (leftTest.IsSemanticallyEqualTo(rightTest))
                    {
                        // (a ? b : c) @ (a ? e : f)
                        // becomes
                        // (a ? (b @ e) : (c @ f))

                        return OptimizeAndRevisit(
                            Expression.Condition(
                                leftTest,
                                Expression.MakeBinary(node.NodeType, leftIfTrue, rightIfTrue),
                                Expression.MakeBinary(node.NodeType, leftIfFalse, rightIfFalse)));
                    }

                    if (leftIfTrue.IsSemanticallyEqualTo(rightIfTrue))
                    {
                        if (leftIfFalse.IsSemanticallyEqualTo(rightIfFalse))
                        {
                            // (a ? b : c) @ (d ? b : c)
                            // becomes
                            // (a @ d)

                            return OptimizeAndRevisit(Expression.MakeBinary(node.NodeType, leftTest, rightTest));
                        }
                        else if (node.NodeType == ExpressionType.Equal)
                        {
                            // (a ? b : c) == (d ? b : f)
                            // becomes
                            // (a && d) || (a && b == f) || (d && b == c) || c == f

                            return OptimizeAndRevisit(
                                Expression.OrElse(
                                    Expression.AndAlso(leftTest, rightTest),
                                    Expression.OrElse(
                                        Expression.AndAlso(leftTest, Expression.Equal(leftIfTrue, rightIfFalse)),
                                        Expression.OrElse(
                                            Expression.AndAlso(rightTest, Expression.Equal(rightIfTrue, leftIfFalse)),
                                            Expression.Equal(leftIfFalse, rightIfFalse)))));
                        }
                        else
                        {
                            // (a ? b : c) != (d ? b : f)
                            // becomes
                            // (a != d && ((a && b != f) || (d && b != c))) || (!a && !d && c != f)

                            return OptimizeAndRevisit(
                                Expression.OrElse(
                                    Expression.AndAlso(
                                        Expression.NotEqual(leftTest, rightTest),
                                        Expression.OrElse(
                                            Expression.AndAlso(
                                                leftTest,
                                                Expression.NotEqual(leftIfTrue, rightIfFalse)),
                                            Expression.AndAlso(
                                                rightTest,
                                                Expression.NotEqual(rightIfTrue, leftIfFalse)))),
                                    Expression.AndAlso(
                                        Expression.Not(leftTest),
                                        Expression.AndAlso(
                                            Expression.Not(rightTest),
                                            Expression.NotEqual(leftIfFalse, rightIfFalse)))));
                        }
                    }
                    else if (leftIfFalse.IsSemanticallyEqualTo(rightIfFalse))
                    {
                        if (node.NodeType == ExpressionType.Equal)
                        {
                            // (a ? b : c) == (d ? e : c)
                            // becomes
                            // (!a && !d) || (a && b == c) || (d && e == c) || b == e

                            return OptimizeAndRevisit(
                                Expression.OrElse(
                                    Expression.AndAlso(Expression.Not(leftTest), Expression.Not(rightTest)),
                                    Expression.OrElse(
                                        Expression.AndAlso(leftTest, Expression.Equal(leftIfTrue, rightIfFalse)),
                                        Expression.OrElse(
                                            Expression.AndAlso(rightTest, Expression.Equal(rightIfTrue, leftIfFalse)),
                                            Expression.Equal(leftIfTrue, rightIfTrue)))));
                        }
                        else
                        {
                            // (a ? b : c) != (d ? e : c)
                            // becomes
                            // (a != d && ((a && b != c) || (d && e != c))) || (!a && !d && b != e)

                            return OptimizeAndRevisit(
                                Expression.OrElse(
                                    Expression.AndAlso(
                                        Expression.NotEqual(leftTest, rightTest),
                                        Expression.OrElse(
                                            Expression.AndAlso(
                                                leftTest,
                                                Expression.NotEqual(leftIfTrue, rightIfFalse)),
                                            Expression.AndAlso(
                                                rightTest,
                                                Expression.NotEqual(rightIfTrue, leftIfFalse)))),
                                    Expression.AndAlso(
                                        Expression.Not(leftTest),
                                        Expression.AndAlso(
                                            Expression.Not(rightTest),
                                            Expression.NotEqual(leftIfTrue, rightIfTrue)))));
                        }
                    }
                }
                else if (leftConditional != null || rightConditional != null)
                {
                    var conditional = leftConditional ?? rightConditional;
                    var comparand = leftConditional == null ? visitedLeft : visitedRight;

                    return OptimizeAndRevisit(
                        Expression.OrElse(
                            Expression.AndAlso(
                                conditional.Test,
                                Expression.MakeBinary(node.NodeType, conditional.IfTrue, comparand)),
                            Expression.AndAlso(
                                Expression.Not(conditional.Test),
                                Expression.MakeBinary(node.NodeType, conditional.IfFalse, comparand))));
                }
            }

            return node.Update(visitedLeft, node.Conversion, visitedRight);
        }
    }
}
