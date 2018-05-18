using Impatient.Query.Infrastructure;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class LateBoundProjectionLeafExpression : Expression, ISemanticHashCodeProvider
    {
        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool CanReduce => true;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public virtual int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            return 0;
        }
    }
}
