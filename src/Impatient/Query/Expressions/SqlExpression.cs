using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    /// <summary>
    /// Any type of Expression that inherits from this type will be considered translatable.
    /// </summary>
    public abstract class SqlExpression : Expression
    {
        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
    }
}
