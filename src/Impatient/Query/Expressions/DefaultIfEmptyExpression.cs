using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class DefaultIfEmptyExpression : ExtraPropertiesExpression
    {
        private static readonly ReadOnlyCollection<string> names
            = new ReadOnlyCollection<string>(new[] { "$empty" });

        public DefaultIfEmptyExpression(Expression expression) : this(expression, Constant(0, typeof(int?)))
        {
        }

        public DefaultIfEmptyExpression(Expression expression, Expression flag) : base(expression)
        {
            if (flag is null)
            {
                throw new ArgumentNullException(nameof(flag));
            }

            Properties = new ReadOnlyCollection<Expression>(new[] { flag });
        }

        public Expression Flag => Properties[0];

        public override ReadOnlyCollection<string> Names => names;

        public override ReadOnlyCollection<Expression> Properties { get; }

        public override ExtraPropertiesExpression Update(Expression expression, IEnumerable<Expression> properties)
        {
            if (expression != Expression || !properties.SequenceEqual(Properties))
            {
                return new DefaultIfEmptyExpression(expression, properties.ElementAtOrDefault(0));
            }

            return this;
        }
    }
}
