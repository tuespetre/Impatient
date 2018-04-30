using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class RelationalQueryExpression : Expression, ISemanticHashCodeProvider
    {
        protected RelationalQueryExpression(SelectExpression selectExpression, Type type)
        {
            SelectExpression = selectExpression ?? throw new ArgumentNullException(nameof(selectExpression));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public SelectExpression SelectExpression { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public virtual int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            return comparer.GetHashCode(SelectExpression);
        }
    }
}
