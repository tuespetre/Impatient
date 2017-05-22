using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class BooleanOptimizingExpressionVisitor : ExpressionVisitor
    {
        private static readonly NotBooleanConstantEliminatingExpressionVisitor notBooleanConstantEliminatingExpressionVisitor = new NotBooleanConstantEliminatingExpressionVisitor();
        private static readonly RedundantNotEliminatingExpressionVisitor redundantNotEliminatingExpressionVisitor = new RedundantNotEliminatingExpressionVisitor();
        private static readonly BinaryExpressionReducingExpressionVisitor binaryExpressionReducingExpressionVisitor = new BinaryExpressionReducingExpressionVisitor();
        private static readonly NotDistributingExpressionVisitor notDistributingExpressionVisitor = new NotDistributingExpressionVisitor();
        private static readonly BinaryFlippingExpressionVisitor binaryFlippingExpressionVisitor = new BinaryFlippingExpressionVisitor();

        public override Expression Visit(Expression node)
        {
            node = notBooleanConstantEliminatingExpressionVisitor.Visit(node);
            node = binaryExpressionReducingExpressionVisitor.Visit(node);
            node = notDistributingExpressionVisitor.Visit(node);
            node = redundantNotEliminatingExpressionVisitor.Visit(node);

            return node;
        }

        private class NotBooleanConstantEliminatingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitUnary(UnaryExpression node)
            {
                var operand = Visit(node.Operand);

                if (node.NodeType == ExpressionType.Not
                    && operand.Type == typeof(bool)
                    && operand is ConstantExpression constantExpression)
                {
                    return constantExpression.Value.Equals(true)
                        ? Expression.Constant(false)
                        : Expression.Constant(true);
                }

                return base.VisitUnary(node);
            }
        }

        private class RedundantNotEliminatingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitUnary(UnaryExpression node)
            {
                var operand = Visit(node.Operand);

                if (node.NodeType == ExpressionType.Not
                    && operand is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Not)
                {
                    return unaryExpression.Operand;
                }

                return base.VisitUnary(node);
            }
        }

        private class BinaryExpressionReducingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitBinary(BinaryExpression node)
            {
                var left = Visit(node.Left);
                var right = Visit(node.Right);

                var leftConstant = left as ConstantExpression;
                var rightConstant = right as ConstantExpression;

                switch (node.NodeType)
                {
                    case ExpressionType.AndAlso:
                    {
                        if (leftConstant != null && rightConstant != null)
                        {
                            if (false.Equals(leftConstant.Value) || false.Equals(rightConstant.Value))
                            {
                                return Expression.Constant(false);
                            }
                            else if (true.Equals(leftConstant.Value) && true.Equals(rightConstant.Value))
                            {
                                return Expression.Constant(true);
                            }
                        }
                        else if (leftConstant != null)
                        {
                            if (false.Equals(leftConstant.Value))
                            {
                                return left;
                            }
                            else if (true.Equals(leftConstant.Value))
                            {
                                return right;
                            }
                        }
                        else if (rightConstant != null)
                        {
                            if (false.Equals(rightConstant.Value))
                            {
                                return right;
                            }
                            else if (true.Equals(rightConstant.Value))
                            {
                                return left;
                            }
                        }

                        break;
                    }

                    case ExpressionType.OrElse:
                    {
                        if (leftConstant != null && rightConstant != null)
                        {
                            if (true.Equals(leftConstant.Value) || true.Equals(rightConstant.Value))
                            {
                                return Expression.Constant(true);
                            }
                            else if (false.Equals(leftConstant.Value) && false.Equals(rightConstant.Value))
                            {
                                return Expression.Constant(false);
                            }
                        }
                        else if (leftConstant != null)
                        {
                            if (true.Equals(leftConstant.Value))
                            {
                                return left;
                            }
                            else if (false.Equals(leftConstant.Value))
                            {
                                return right;
                            }
                        }
                        else if (rightConstant != null)
                        {
                            if (true.Equals(rightConstant.Value))
                            {
                                return right;
                            }
                            else if (false.Equals(rightConstant.Value))
                            {
                                return left;
                            }
                        }

                        break;
                    }

                    case ExpressionType.Equal:
                    {
                        if (leftConstant != null && rightConstant != null)
                        {
                            if (leftConstant.Value == rightConstant.Value)
                            {
                                return Expression.Constant(true);
                            }
                            else
                            {
                                return Expression.Constant(false);
                            }
                        }
                        else if (leftConstant != null)
                        {
                            if (true.Equals(leftConstant.Value))
                            {
                                return right;
                            }
                            else if (false.Equals(leftConstant.Value))
                            {
                                return Expression.Not(right);
                            }
                        }
                        else if (rightConstant != null)
                        {
                            if (true.Equals(rightConstant.Value))
                            {
                                return left;
                            }
                            else if (false.Equals(rightConstant.Value))
                            {
                                return Expression.Not(left);
                            }
                        }

                        break;
                    }

                    case ExpressionType.NotEqual:
                    {
                        if (leftConstant != null && rightConstant != null)
                        {
                            if (leftConstant.Value == rightConstant.Value)
                            {
                                return Expression.Constant(false);
                            }
                            else
                            {
                                return Expression.Constant(true);
                            }
                        }
                        else if (leftConstant != null)
                        {
                            if (false.Equals(leftConstant.Value))
                            {
                                return right;
                            }
                            else if (true.Equals(leftConstant.Value))
                            {
                                return Expression.Not(right);
                            }
                        }
                        else if (rightConstant != null)
                        {
                            if (false.Equals(rightConstant.Value))
                            {
                                return left;
                            }
                            else if (true.Equals(rightConstant.Value))
                            {
                                return Expression.Not(left);
                            }
                        }

                        break;
                    }
                }

                return node.Update(left, node.Conversion, right);
            }
        }

        private class NotDistributingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitUnary(UnaryExpression node)
            {
                var operand = Visit(node.Operand);

                if (node.NodeType == ExpressionType.Not)
                {
                    switch (operand)
                    {
                        case BinaryExpression binaryExpression:
                        {
                            return binaryFlippingExpressionVisitor.Visit(binaryExpression);
                        }

                        case ConstantExpression constantExpression:
                        {
                            return true.Equals(constantExpression.Value)
                                ? Expression.Constant(false)
                                : Expression.Constant(true);
                        }

                        case UnaryExpression unaryExpression
                        when unaryExpression.NodeType == ExpressionType.Not:
                        {
                            return unaryExpression.Operand;
                        }
                    }
                }

                return node.Update(operand);
            }
        }

        private class BinaryFlippingExpressionVisitor : ExpressionVisitor
        {
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
}
