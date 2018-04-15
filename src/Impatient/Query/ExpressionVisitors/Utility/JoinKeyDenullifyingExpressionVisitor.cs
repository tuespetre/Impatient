using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// An <see cref="ExpressionVisitor"/> that marks all instances of
    /// <see cref="SqlColumnExpression"/> as non-nullable. This avoids the
    /// overhead of unnecessary null-checking during join operations (if the
    /// keys are being checked during a join, it means the objects are definitely
    /// not null.)
    /// </summary>
    public class JoinKeyDenullifyingExpressionVisitor : ExpressionVisitor
    {
        public static JoinKeyDenullifyingExpressionVisitor Instance { get; }
            = new JoinKeyDenullifyingExpressionVisitor();

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case SqlColumnExpression sqlColumnExpression:
                {
                    return new SqlColumnExpression(
                        sqlColumnExpression.Table,
                        sqlColumnExpression.ColumnName,
                        sqlColumnExpression.Type,
                        isNullable: false);
                }

                case BinaryExpression binaryExpression
                when node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual:
                {
                    return base.VisitBinary(binaryExpression);
                }
                
                case NewArrayExpression _:
                case UnaryExpression _ when node.NodeType == ExpressionType.Convert:
                {
                    return base.Visit(node);
                }

                default:
                {
                    return node;
                }
            }
        }
    }
}
