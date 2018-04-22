using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    // TODO: Verify how this works with result selectors like SelectMany
    internal class QueryOptionsAnnotatingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            var visitor = new QueryOptionsDiscoveringExpressionVisitor();

            node = visitor.Visit(node);

            var queryTrackingBehavior = default(QueryTrackingBehavior);
            var ignoreQueryFilters = false;
            var useRelationalNullSemantics = false;

            if (node is QueryOptionsExpression queryOptionsExpression)
            {
                node = queryOptionsExpression.Expression;
                queryTrackingBehavior = queryOptionsExpression.QueryTrackingBehavior;
                ignoreQueryFilters = queryOptionsExpression.IgnoreQueryFilters;
                useRelationalNullSemantics = queryOptionsExpression.UseRelationalNullSemantics;
            }

            if (visitor.QueryTrackingBehavior.HasValue)
            {
                queryTrackingBehavior = visitor.QueryTrackingBehavior.Value;
            }

            if (visitor.IgnoreQueryFilters)
            {
                ignoreQueryFilters = true;
            }

            return new QueryOptionsExpression(
                node, 
                queryTrackingBehavior, 
                ignoreQueryFilters,
                useRelationalNullSemantics);
        }

        private class QueryOptionsDiscoveringExpressionVisitor : ExpressionVisitor
        {
            public QueryTrackingBehavior? QueryTrackingBehavior { get; private set; }

            public bool IgnoreQueryFilters { get; private set; }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions))
                {
                    switch (node.Method.Name)
                    {
                        case nameof(EntityFrameworkQueryableExtensions.AsNoTracking):
                        {
                            var visited = base.Visit(node.Arguments[0]);

                            QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;

                            return visited;
                        }

                        case nameof(EntityFrameworkQueryableExtensions.AsTracking):
                        {
                            var visited = base.Visit(node.Arguments[0]);

                            QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.TrackAll;

                            return visited;
                        }

                        case nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters):
                        {
                            var visited = base.Visit(node.Arguments[0]);

                            IgnoreQueryFilters = true;

                            return visited;
                        }
                    }
                }

                return base.VisitMethodCall(node);
            }
        }
    }
}
