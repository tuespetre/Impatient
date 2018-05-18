using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using System.Linq;
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

            var projection = subquery.Projection.Flatten().Body;
            var leafGatherer = new ProjectionLeafGatheringExpressionVisitor();
            leafGatherer.Visit(projection);

            if (leafGatherer.GatheredExpressions.Count == 1 
                && string.IsNullOrEmpty(leafGatherer.GatheredExpressions.Keys.Single())
                && !(projection is SqlColumnExpression || projection is SqlAliasExpression))
            {
                subquery
                    = subquery.UpdateProjection(
                        new ServerProjectionExpression(
                            new SqlAliasExpression(
                                subquery.Projection.ResultLambda.Body,
                                "$c")));
            }

            // Strip DefaultIfEmptyExpressions out because FOR JSON will leave out the null values

            var strippingVisitor = new DefaultIfEmptyStrippingExpressionVisitor();

            subquery = strippingVisitor.VisitAndConvert(subquery, nameof(Format));

            // Visit and print the subquery

            subquery = visitor.VisitAndConvert(subquery, nameof(Format));

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
}
