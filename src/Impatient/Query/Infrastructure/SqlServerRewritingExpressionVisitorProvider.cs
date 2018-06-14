using Impatient.Query.ExpressionVisitors.Rewriting;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public class SqlServerRewritingExpressionVisitorProvider : IProviderSpecificRewritingExpressionVisitorProvider
    {
        public SqlServerRewritingExpressionVisitorProvider()
        {
        }

        public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            yield return new SqlServerObjectToStringRewritingExpressionVisitor();

            yield return new SqlServerStringToNumberAsciiRewritingExpressionVisitor();

            yield return new SqlServerCountRewritingExpressionVisitor();

            yield return new SqlServerMathMethodRewritingExpressionVisitor();

            yield return new SqlServerJsonMemberRewritingExpressionVisitor();

            yield return new SqlServerStringJoinRewritingExpressionVisitor(context);
        }
    }
}
