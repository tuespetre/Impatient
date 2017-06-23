using System;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IReadValueExpressionFactory
    {
        bool CanReadType(Type type);

        Expression CreateExpression(Expression source, Expression reader, int index);
    }
}
