using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class ExtraPropertiesExpression : Expression, ISemanticallyHashable
    {
        public ExtraPropertiesExpression(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; }

        public abstract IReadOnlyList<string> Names { get; }

        public abstract ReadOnlyCollection<Expression> Properties { get; }

        public override Type Type => Expression.Type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool CanReduce => true;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);
            var properties = visitor.Visit(Properties);

            return Update(expression, properties);
        }

        public abstract ExtraPropertiesExpression Update(Expression expression, IEnumerable<Expression> properties);

        public virtual int GetSemanticHashCode() => 0;
    }
}
