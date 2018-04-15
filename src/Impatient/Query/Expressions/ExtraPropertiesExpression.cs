using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class ExtraPropertiesExpression : AnnotationExpression
    {
        public ExtraPropertiesExpression(Expression expression) : base(expression)
        {
        }

        public abstract IReadOnlyList<string> Names { get; }

        public abstract ReadOnlyCollection<Expression> Properties { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);
            var properties = visitor.Visit(Properties);

            return Update(expression, properties);
        }

        public abstract ExtraPropertiesExpression Update(Expression expression, IEnumerable<Expression> properties);

        public override int GetSemanticHashCode() => 0;
    }
}
