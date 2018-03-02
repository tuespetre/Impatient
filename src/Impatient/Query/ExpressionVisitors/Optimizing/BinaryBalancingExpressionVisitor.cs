using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class BinaryBalancingExpressionVisitor : ExpressionVisitor
    {
        public static BinaryBalancingExpressionVisitor Instance { get; } = new BinaryBalancingExpressionVisitor();

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (!IsCommutativeBinaryNodeType(node.NodeType))
            {
                return base.VisitBinary(node);
            }

            var splitNodes = node.SplitNodes(node.NodeType).Select(Visit).ToArray();

            while (splitNodes.Length > 1)
            {
                splitNodes
                    = (from x in splitNodes.Select((n, i) => new { n, i })
                       group x.n by x.i - (x.i % 2) into p
                       select p.Count() == 1
                          ? p.Single()
                          : Expression.MakeBinary(
                              node.NodeType,
                              p.ElementAt(0),
                              p.ElementAt(1))).ToArray();
            }

            return splitNodes.Single();
        }

        private static bool IsCommutativeBinaryNodeType(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Add:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Multiply:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Subtract:
                {
                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }
    }
}
