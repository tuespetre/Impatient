using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class SqlParameterRewritingExpressionVisitor2 : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case null:
                case LambdaExpression _:
                {
                    return node;
                }

                default:
                {
                    if (node.Type.IsScalarType())
                    {
                        var verifyingVisitor = new ConstraintVerifyingExpressionVisitor();

                        verifyingVisitor.Visit(node);

                        if (verifyingVisitor.Verified)
                        {
                            return new SqlParameterExpression(node);
                        }
                    }

                    return base.Visit(node);
                }
            }
        }

        private class ConstraintVerifyingExpressionVisitor : ExpressionVisitor
        {
            private bool foundStaticReference;

            private bool foundExtension;

            public bool Verified => foundStaticReference && !foundExtension;

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression == null)
                {
                    foundStaticReference = true;
                }

                return base.VisitMember(node);
            }

            protected override Expression VisitExtension(Expression node)
            {
                foundExtension = true;

                return base.VisitExtension(node);
            }
        }
    }
}
