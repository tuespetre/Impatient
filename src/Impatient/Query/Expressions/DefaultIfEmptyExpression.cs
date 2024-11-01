using Impatient.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions;

public class DefaultIfEmptyExpression : ExtraPropertiesExpression
{
    private static readonly ReadOnlyCollection<string> names = new(["$empty"]);

    public DefaultIfEmptyExpression(Expression expression) : this(expression, Constant(0, typeof(int?)))
    {
    }

    public DefaultIfEmptyExpression(Expression expression, Expression flag) : base(expression)
    {
        ArgumentNullException.ThrowIfNull(flag);
        //ArgumentOutOfRangeException.ThrowIfNotEqual(true, flag.Type.IsNullableType() || !flag.Type.IsValueType);

        Properties = new ReadOnlyCollection<Expression>([flag]);
    }

    public Expression Flag => Properties[0];

    public override ReadOnlyCollection<string> Names => names;

    public override ReadOnlyCollection<Expression> Properties { get; }

    public override Expression Reduce()
    {
        return Condition(
            Equal(Flag, Constant(null, Flag.Type)),
            Default(Expression.Type),
            Expression);
    }

    public override ExtraPropertiesExpression Update(Expression expression, IEnumerable<Expression> properties)
    {
        if (expression != Expression || !properties.SequenceEqual(Properties))
        {
            return new DefaultIfEmptyExpression(expression, properties.ElementAtOrDefault(0));
        }

        return this;
    }
}
