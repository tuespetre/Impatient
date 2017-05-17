using Impatient.Query.Expressions;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class RelationalGroupingAggregationRewritingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if ((node.Method.DeclaringType == typeof(Queryable)
                    || node.Method.DeclaringType == typeof(Enumerable))
                && arguments[0] is RelationalGroupingExpression relationalGrouping)
            {
                switch (node.Method.Name)
                {
                    case nameof(Queryable.Average):
                    case nameof(Queryable.Max) when node.Method.ReturnType.IsScalarType():
                    case nameof(Queryable.Min) when node.Method.ReturnType.IsScalarType():
                    case nameof(Queryable.Sum):
                    {
                        var selector = relationalGrouping.ElementSelector;

                        if (node.Arguments.Count == 2)
                        {
                            selector = node.Arguments[1].UnwrapLambda().ExpandParameters(selector);
                        }

                        if (!selector.IsTranslatable())
                        {
                            break;
                        }

                        return new SqlAggregateExpression(
                            node.Method.Name == nameof(Queryable.Average)
                                ? "AVG"
                                : node.Method.Name.ToUpperInvariant(),
                            selector,
                            node.Method.ReturnType,
                            relationalGrouping is DistinctRelationalGroupingExpression
                                && node.Arguments.Count == 1);
                    }

                    case nameof(Queryable.Count):
                    case nameof(Queryable.LongCount):
                    {
                        var selector = relationalGrouping.ElementSelector;

                        if (node.Arguments.Count == 2)
                        {
                            var predicate = node.Arguments[1].UnwrapLambda().ExpandParameters(selector);

                            if (!predicate.IsTranslatable())
                            {
                                break;
                            }

                            selector
                                = Expression.Condition(
                                    predicate,
                                    Expression.Constant(1, typeof(int?)),
                                    Expression.Constant(null, typeof(int?)));
                        }

                        return new SqlAggregateExpression(
                            node.Method.Name == nameof(Queryable.Count)
                                ? "COUNT"
                                : "COUNT_BIG",
                            selector.Type.IsScalarType()
                                ? selector
                                : new SqlFragmentExpression("*", selector.Type),
                            node.Method.ReturnType,
                            relationalGrouping is DistinctRelationalGroupingExpression
                                && node.Arguments.Count == 1);
                    }

                    case nameof(Queryable.Distinct):
                    {
                        if (relationalGrouping is DistinctRelationalGroupingExpression)
                        {
                            return relationalGrouping;
                        }
                        else
                        {
                            return new DistinctRelationalGroupingExpression(
                                relationalGrouping.UnderlyingQuery,
                                relationalGrouping.KeySelector,
                                relationalGrouping.ElementSelector);
                        }
                    }

                    case nameof(Queryable.Select):
                    {
                        var selectorLambda = node.Arguments[1].UnwrapLambda();

                        if (selectorLambda.Parameters.Count == 2)
                        {
                            // index parameter not supported
                            break;
                        }

                        var selectorBody
                            = selectorLambda
                                .ExpandParameters(relationalGrouping.ElementSelector);

                        if (!selectorBody.IsTranslatable())
                        {
                            break;
                        }

                        return new RelationalGroupingExpression(
                            relationalGrouping.UnderlyingQuery,
                            relationalGrouping.KeySelector,
                            selectorBody);
                    }
                }
            }

            return node.Update(@object, arguments);
        }
    }
}
