using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IComplexTypeSubqueryFormatter
    {
        SelectExpression Format(SelectExpression subquery, IDbCommandExpressionBuilder builder, ExpressionVisitor visitor);
    }
}
