using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Expressions
{
    public class SimpleExtraPropertiesExpression : ExtraPropertiesExpression
    {
        public SimpleExtraPropertiesExpression(
            Expression expression,
            IEnumerable<string> names,
            IEnumerable<Expression> properties) 
            : this(
                  expression, 
                  new ReadOnlyCollection<string>(names.ToArray()), 
                  new ReadOnlyCollection<Expression>(properties.ToArray()))
        {
        }

        private SimpleExtraPropertiesExpression(
            Expression expression,
            ReadOnlyCollection<string> names,
            ReadOnlyCollection<Expression> properties) : base(expression)
        {
            Names = names;
            Properties = properties;
        }

        public override ReadOnlyCollection<string> Names { get; }

        public override ReadOnlyCollection<Expression> Properties { get; }

        public override ExtraPropertiesExpression Update(Expression expression, IEnumerable<Expression> properties)
        {
            if (expression != Expression || !properties.SequenceEqual(Properties))
            {
                return new SimpleExtraPropertiesExpression(expression, Names, properties);
            }

            return this;
        }

        public SimpleExtraPropertiesExpression SetProperty(string name, Expression expression)
        {
            var names = Names.ToList();
            var properties = Properties.ToList();
            var found = false;

            for (var i = 0; i < names.Count; i++)
            {
                if (names[i] == name)
                {
                    Debug.Assert(properties[i].Type.IsAssignableFrom(expression.Type));

                    properties[i] = expression;
                    found = true;
                }
            }

            if (!found)
            {
                names.Add(name);
                properties.Add(expression);
            }

            return new SimpleExtraPropertiesExpression(
                Expression,
                names,
                new ReadOnlyCollection<Expression>(properties));
        }
    }
}
