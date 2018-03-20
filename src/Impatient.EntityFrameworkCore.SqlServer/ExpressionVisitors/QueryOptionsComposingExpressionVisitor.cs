using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class QueryOptionsComposingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case QueryOptionsExpression queryOptionsExpression:
                {
                    var visitor
                        = new QueryOptionsApplyingExpressionVisitor(
                            queryOptionsExpression.QueryTrackingBehavior,
                            queryOptionsExpression.IgnoreQueryFilters);

                    return visitor.Visit(queryOptionsExpression.Expression);
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }


        private class QueryOptionsApplyingExpressionVisitor : ExpressionVisitor
        {
            private QueryTrackingBehavior queryTrackingBehavior;
            private bool ignoreQueryFilters;

            public QueryOptionsApplyingExpressionVisitor(QueryTrackingBehavior queryTrackingBehavior, bool ignoreQueryFilters)
            {
                this.queryTrackingBehavior = queryTrackingBehavior;
                this.ignoreQueryFilters = ignoreQueryFilters;
            }

            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case SelectExpression selectExpression
                    when ignoreQueryFilters && selectExpression.Predicate is QueryFilterExpression:
                    {
                        return base.Visit(selectExpression.RemovePredicate());
                    }

                    case BinaryExpression binaryExpression
                    when ignoreQueryFilters && binaryExpression.Left is QueryFilterExpression:
                    {
                        return base.Visit(binaryExpression.Right);
                    }

                    case BinaryExpression binaryExpression
                    when ignoreQueryFilters && binaryExpression.Right is QueryFilterExpression:
                    {
                        return base.Visit(binaryExpression.Left);
                    }

                    case EntityMaterializationExpression entityMaterializationExpression:
                    {
                        var state 
                            = queryTrackingBehavior == QueryTrackingBehavior.TrackAll 
                                ? EntityState.Unchanged 
                                : EntityState.Detached;

                        return entityMaterializationExpression.UpdateState(state);
                    }

                    default:
                    {
                        return base.Visit(node);
                    }
                }
            }
        }
    }
}
