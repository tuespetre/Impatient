using Impatient.Query.Infrastructure;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    /// <summary>
    /// Any type of Expression that inherits from this type will be considered translatable.
    /// </summary>
    public abstract class SqlExpression : Expression, ISemanticallyHashable
    {
        public virtual bool IsNullable { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public virtual int GetSemanticHashCode() => IsNullable.GetHashCode();
    }
}
