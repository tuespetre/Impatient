using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class AccessorDistributingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Index:
                {
                    switch (node.Left)
                    {
                        case BinaryExpression binaryExpression
                        when binaryExpression.NodeType == ExpressionType.Coalesce:
                        {
                            return Visit(
                                Expression.Condition(
                                    Expression.NotEqual(binaryExpression.Left, Expression.Constant(null, binaryExpression.Type)),
                                    node.Update(binaryExpression.Left, node.Conversion, node.Right),
                                    node.Update(binaryExpression.Right, node.Conversion, node.Right)));
                        }

                        case ConditionalExpression conditionalExpression:
                        {
                            return Visit(
                                conditionalExpression.Update(
                                    conditionalExpression.Test,
                                    node.Update(conditionalExpression.IfTrue, node.Conversion, node.Right),
                                    node.Update(conditionalExpression.IfFalse, node.Conversion, node.Right)));
                        }

                        default:
                        {
                            return base.VisitBinary(node);
                        }
                    }
                }

                default:
                {
                    return base.VisitBinary(node);
                }
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            switch (node.Expression)
            {
                case BinaryExpression binaryExpression
                when binaryExpression.NodeType == ExpressionType.Coalesce:
                {
                    return Visit(
                        Expression.Condition(
                        Expression.NotEqual(binaryExpression.Left, Expression.Constant(null, binaryExpression.Type)),
                            node.Update(binaryExpression.Left),
                            node.Update(binaryExpression.Right)));
                }

                case ConditionalExpression conditionalExpression:
                {
                    return Visit(
                        conditionalExpression.Update(
                            conditionalExpression.Test,
                            node.Update(conditionalExpression.IfTrue),
                            node.Update(conditionalExpression.IfFalse)));
                }

                default:
                {
                    return base.VisitMember(node);
                }
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            switch (node.Object)
            {
                case BinaryExpression binaryExpression
                when binaryExpression.NodeType == ExpressionType.Coalesce:
                {
                    return Visit(
                        Expression.Condition(
                            Expression.NotEqual(binaryExpression.Left, Expression.Constant(null, binaryExpression.Type)),
                            node.Update(binaryExpression.Left, node.Arguments),
                            node.Update(binaryExpression.Right, node.Arguments)));
                }

                case ConditionalExpression conditionalExpression:
                {
                    return Visit(
                        conditionalExpression.Update(
                            conditionalExpression.Test,
                            node.Update(conditionalExpression.IfTrue, node.Arguments),
                            node.Update(conditionalExpression.IfFalse, node.Arguments)));
                }

                default:
                {
                    return base.VisitMethodCall(node);
                }
            }
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            switch (node.Object)
            {
                case BinaryExpression binaryExpression
                when binaryExpression.NodeType == ExpressionType.Coalesce:
                {
                    return Visit(
                        Expression.Condition(
                            Expression.NotEqual(binaryExpression.Left, Expression.Constant(null, binaryExpression.Type)),
                            node.Update(binaryExpression.Left, node.Arguments),
                            node.Update(binaryExpression.Right, node.Arguments)));
                }

                case ConditionalExpression conditionalExpression:
                {
                    return Visit(
                        conditionalExpression.Update(
                            conditionalExpression.Test,
                            node.Update(conditionalExpression.IfTrue, node.Arguments),
                            node.Update(conditionalExpression.IfFalse, node.Arguments)));
                }

                default:
                {
                    return base.VisitIndex(node);
                }
            }
        }
    }
}
