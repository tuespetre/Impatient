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

            visitor.Visit(node);

            var queryTrackingBehavior = default(QueryTrackingBehavior);
            var ignoreQueryFilters = false;

            if (node is QueryOptionsExpression queryOptionsExpression)
            {
                queryTrackingBehavior = queryOptionsExpression.QueryTrackingBehavior;
                ignoreQueryFilters = queryOptionsExpression.IgnoreQueryFilters;
            }

            if (visitor.FoundAsTracking)
            {
                queryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
            else if (visitor.FoundAsNoTracking)
            {
                queryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }

            if (visitor.FoundIgnoreQueryFilters)
            {
                ignoreQueryFilters = true;
            }

            return new QueryOptionsExpression(node, queryTrackingBehavior, ignoreQueryFilters);
        }

        private class QueryOptionsDiscoveringExpressionVisitor : ExpressionVisitor
        {
            public bool FoundAsNoTracking { get; private set; }

            public bool FoundAsTracking { get; private set; }

            public bool FoundIgnoreQueryFilters { get; private set; }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions))
                {
                    switch (node.Method.Name)
                    {
                        case nameof(EntityFrameworkQueryableExtensions.AsNoTracking):
                        {
                            FoundAsNoTracking = true;
                            break;
                        }

                        case nameof(EntityFrameworkQueryableExtensions.AsTracking):
                        {
                            FoundAsTracking = true;
                            break;
                        }

                        case nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters):
                        {
                            FoundIgnoreQueryFilters = true;
                            break;
                        }
                    }
                }

                return base.VisitMethodCall(node);
            }
        }
    }
}
