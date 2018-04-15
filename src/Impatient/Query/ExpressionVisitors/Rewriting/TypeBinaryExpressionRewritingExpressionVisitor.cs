using Impatient.Query.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class TypeBinaryExpressionRewritingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.TypeEqual:
                case ExpressionType.TypeIs:
                {
                    if (node.Expression.UnwrapAnnotations() is PolymorphicExpression polymorphicExpression)
                    {
                        return Visit(
                            polymorphicExpression
                                .Filter(node.TypeOperand)
                                .Descriptors
                                .Select(d => d.Test.ExpandParameters(polymorphicExpression.Row))
                                .Aggregate(Expression.OrElse));
                    }

                    goto default;
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
