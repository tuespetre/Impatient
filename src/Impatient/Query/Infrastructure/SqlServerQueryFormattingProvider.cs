using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Projection;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure;

public class SqlServerQueryFormattingProvider : IQueryFormattingProvider
{
    public string FormatIdentifier(string identifier) => $"[{identifier}]";

    public string FormatParameterName(string name) => $"@{name}";

    public bool SupportsComplexTypeSubqueries => true;

    public SelectExpression FormatComplexTypeSubquery(
        SelectExpression subquery, 
        IDbCommandExpressionBuilder builder, 
        ExpressionVisitor visitor)
    {
        builder.Append("(");

        builder.IncreaseIndent();
        builder.AppendLine();

        // Strip DefaultIfEmptyExpressions out because FOR JSON will leave out the null values

        var strippingVisitor = new DefaultIfEmptyStrippingExpressionVisitor();

        subquery = strippingVisitor.VisitAndConvert(subquery, nameof(FormatComplexTypeSubquery));

        // Visit and print the subquery

        subquery = visitor.VisitAndConvert(subquery, nameof(FormatComplexTypeSubquery));

        builder.AppendLine();
        builder.Append("FOR JSON PATH");

        builder.DecreaseIndent();
        builder.AppendLine();

        builder.Append(")");

        return subquery;
    }

    private class DefaultIfEmptyStrippingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            if (node is DefaultIfEmptyExpression defaultIfEmptyExpression)
            {
                return Visit(defaultIfEmptyExpression.Expression);
            }

            return base.VisitExtension(node);
        }
    }
}
