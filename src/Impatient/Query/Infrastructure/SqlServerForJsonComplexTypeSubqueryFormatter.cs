using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public class SqlServerForJsonComplexTypeSubqueryFormatter : IComplexTypeSubqueryFormatter
    {
        public SelectExpression Format(SelectExpression subquery, IDbCommandExpressionBuilder builder, ExpressionVisitor visitor)
        {
            builder.Append("(");

            builder.IncreaseIndent();
            builder.AppendLine();

            subquery = visitor.VisitAndConvert(subquery, nameof(Format));

            builder.AppendLine();
            builder.Append("FOR JSON PATH");

            builder.DecreaseIndent();
            builder.AppendLine();

            builder.Append(")");

            return subquery;
        }
    }
}
