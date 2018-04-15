using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

                    return visitor.Visit(queryOptionsExpression);
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

                    case MethodCallExpression methodCallExpression
                    when ignoreQueryFilters
                        && methodCallExpression.Method.Name == nameof(Queryable.Where)
                        && methodCallExpression.Arguments[1] is UnaryExpression unaryExpression
                        && unaryExpression.Operand is LambdaExpression lambdaExpression
                        && lambdaExpression.Body is QueryFilterExpression:
                    {
                        return base.Visit(methodCallExpression.Arguments[0]);
                    }

                    case EntityMaterializationExpression entityMaterializationExpression:
                    {
                        var identityMapMode 
                            = queryTrackingBehavior == QueryTrackingBehavior.TrackAll 
                                ? IdentityMapMode.StateManager
                                : IdentityMapMode.IdentityMapWithFixup;

                        return base.Visit(
                            entityMaterializationExpression
                                .UpdateIdentityMapMode(identityMapMode));
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
