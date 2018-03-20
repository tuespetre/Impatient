using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// An <see cref="ExpressionVisitor"/> that inverts all boolean expressions it encounters.
    /// </summary>
    public class BinaryInvertingExpressionVisitor : ExpressionVisitor
    {
        public static BinaryInvertingExpressionVisitor Instance { get; } = new BinaryInvertingExpressionVisitor();

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case BinaryExpression binaryExpression:
                {
                    switch (binaryExpression.NodeType)
                    {
                        case ExpressionType.AndAlso:
                        {
                            return Expression.MakeBinary(
                                ExpressionType.OrElse,
                                Visit(binaryExpression.Left),
                                Visit(binaryExpression.Right));
                        }

                        case ExpressionType.OrElse:
                        {
                            return Expression.MakeBinary(
                                ExpressionType.AndAlso,
                                Visit(binaryExpression.Left),
                                Visit(binaryExpression.Right));
                        }

                        case ExpressionType.LessThan:
                        {
                            return Expression.MakeBinary(
                                ExpressionType.GreaterThanOrEqual,
                                binaryExpression.Left,
                                binaryExpression.Right);
                        }

                        case ExpressionType.LessThanOrEqual:
                        {
                            return Expression.MakeBinary(
                                ExpressionType.GreaterThan,
                                binaryExpression.Left,
                                binaryExpression.Right);
                        }

                        case ExpressionType.GreaterThan:
                        {
                            return Expression.MakeBinary(
                                ExpressionType.LessThanOrEqual,
                                binaryExpression.Left,
                                binaryExpression.Right);
                        }

                        case ExpressionType.GreaterThanOrEqual:
                        {
                            return Expression.MakeBinary(
                                ExpressionType.LessThan,
                                binaryExpression.Left,
                                binaryExpression.Right);
                        }

                        case ExpressionType.Equal:
                        {
                            return Expression.MakeBinary(
                                ExpressionType.NotEqual,
                                binaryExpression.Left,
                                binaryExpression.Right);
                        }

                        case ExpressionType.NotEqual:
                        {
                            return Expression.MakeBinary(
                                ExpressionType.Equal,
                                binaryExpression.Left,
                                binaryExpression.Right);
                        }
                    }

                    return binaryExpression;
                }

                default:
                {
                    if (node.Type == typeof(bool))
                    {
                        return Expression.Not(node);
                    }

                    return node;
                }
            }
        }
    }
}
