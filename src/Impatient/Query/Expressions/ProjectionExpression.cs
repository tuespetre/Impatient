using Impatient.Query.Infrastructure;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class ProjectionExpression : Expression, ISemanticHashCodeProvider
    {
        public abstract LambdaExpression ResultLambda { get; }

        public abstract LambdaExpression Flatten();

        public abstract ProjectionExpression Merge(LambdaExpression lambda);

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public virtual int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            return comparer.GetHashCode(ResultLambda);
        }
    }
}
