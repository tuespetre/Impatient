using Impatient.Query.ExpressionVisitors.Rewriting;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure;

public class SqlServerRewritingExpressionVisitorProvider : IProviderSpecificRewritingExpressionVisitorProvider
{
    public SqlServerRewritingExpressionVisitorProvider()
    {
    }

    public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
    {
        // this one just tweaks a SqlExpression slightly

        yield return new SqlServerCountRewritingExpressionVisitor();

        // these should be rewritten as member/method translators instead of complete visitors

        yield return new SqlServerObjectToStringRewritingExpressionVisitor();

        yield return new SqlServerStringToNumberAsciiRewritingExpressionVisitor();

        yield return new SqlServerMathMethodRewritingExpressionVisitor();

        yield return new SqlServerJsonMemberRewritingExpressionVisitor();

        yield return new SqlServerStringJoinRewritingExpressionVisitor(context);
    }
}
