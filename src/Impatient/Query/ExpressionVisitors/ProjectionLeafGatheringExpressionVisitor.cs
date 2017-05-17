using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class ProjectionLeafGatheringExpressionVisitor : ProjectionExpressionVisitor
    {
        public IDictionary<string, Expression> GatheredExpressions { get; } = new Dictionary<string, Expression>();

        protected override Expression VisitLeaf(Expression node)
        {
            var name = string.Join(".", CurrentPath.Reverse());

            GatheredExpressions[name] = node;

            return node;
        }
    }
}
