using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class ProjectionExpression : Expression
    {
        public abstract LambdaExpression ResultLambda { get; }

        public abstract LambdaExpression Flatten();

        public abstract ProjectionExpression Merge(LambdaExpression lambda);

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
    }
}
