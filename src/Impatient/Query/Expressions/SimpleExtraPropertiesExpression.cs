using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SimpleExtraPropertiesExpression : ExtraPropertiesExpression
    {
        public SimpleExtraPropertiesExpression(
            Expression expression,
            IEnumerable<string> names,
            IEnumerable<Expression> properties) : base(expression)
        {
            Names = names.ToArray();
            Properties = new ReadOnlyCollection<Expression>(properties.ToArray());
        }

        public override IReadOnlyList<string> Names { get; }

        public override ReadOnlyCollection<Expression> Properties { get; }

        public override ExtraPropertiesExpression Update(Expression expression, IEnumerable<Expression> properties)
        {
            if (expression != Expression || !properties.SequenceEqual(Properties))
            {
                return new SimpleExtraPropertiesExpression(expression, Names, properties);
            }

            return this;
        }

        public SimpleExtraPropertiesExpression AddProperty(string name, Expression expression)
        {
            return new SimpleExtraPropertiesExpression(
                Expression,
                Names.Append(name),
                Properties.Append(expression));
        }
    }
}
