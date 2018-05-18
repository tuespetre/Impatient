using Impatient.Metadata;
using Impatient.Query.ExpressionVisitors.Rewriting;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Composing
{
    public class KeyEqualityComposingExpressionVisitor : KeyEqualityRewritingExpressionVisitor
    {
        public KeyEqualityComposingExpressionVisitor(DescriptorSet descriptorSet) : base(descriptorSet)
        {
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            // We only want to rewrite key equality for method calls to Join/GroupJoin/etc.
            // during composition. If we rewrite key equality in other expressions like
            // a where clause, the optimizing visitors might 'optimize' away some null checks
            // that would be semantically incorrect.

            var left = VisitAndConvert(node.Left, nameof(VisitBinary));
            var right = VisitAndConvert(node.Right, nameof(VisitBinary));
            var conversion = VisitAndConvert(node.Conversion, nameof(VisitBinary));

            return node.Update(left, conversion, right);
        }
    }
}
