using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IQueryFormattingProvider
    {
        string FormatIdentifier(string identifier);

        string FormatParameterName(string name);

        bool SupportsComplexTypeSubqueries { get; }

        SelectExpression FormatComplexTypeSubquery(
            SelectExpression subquery, 
            IDbCommandExpressionBuilder builder, 
            ExpressionVisitor visitor);
    }
}
