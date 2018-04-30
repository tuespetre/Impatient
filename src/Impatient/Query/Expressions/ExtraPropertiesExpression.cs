using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class ExtraPropertiesExpression : Expression, ISemanticHashCodeProvider
    {
        public ExtraPropertiesExpression(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; }

        public abstract ReadOnlyCollection<string> Names { get; }

        public abstract ReadOnlyCollection<Expression> Properties { get; }

        public override Type Type => Expression.Type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool CanReduce => true;

        public override Expression Reduce() => Expression;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);
            var properties = visitor.Visit(Properties);

            return Update(expression, properties);
        }

        public abstract ExtraPropertiesExpression Update(Expression expression, IEnumerable<Expression> properties);

        public virtual int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = comparer.GetHashCode(Expression);

                for (var i = 0; i < Names.Count; i++)
                {
                    hash = (hash * 16777619) ^ Names[i].GetHashCode();
                    hash = (hash * 16777619) ^ comparer.GetHashCode(Properties[i]);
                }

                return hash;
            }
        }
    }
}
