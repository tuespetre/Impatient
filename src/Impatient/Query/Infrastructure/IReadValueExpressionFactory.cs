using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IReadValueExpressionFactory
    {
        bool CanReadExpression(Expression expression);

        Expression CreateExpression(Expression source, Expression reader, int index);
    }
}
