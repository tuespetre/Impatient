using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting;

public class GroupingAggregationRewritingExpressionVisitor : ExpressionVisitor
{
    private readonly ExpressionTranslatabilityAnalyzer translatabilityAnalyzer;

    public GroupingAggregationRewritingExpressionVisitor(
        ExpressionTranslatabilityAnalyzer translatabilityAnalyzer)
    {
        this.translatabilityAnalyzer
            = translatabilityAnalyzer
                ?? throw new ArgumentNullException(nameof(translatabilityAnalyzer));
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        if (node.Method.IsQueryableOrEnumerableMethod()
            && !node.ContainsNonLambdaDelegates()
            && arguments.FirstOrDefault() is GroupByResultExpression relationalGrouping)
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
                        selector = arguments[1].UnwrapLambda().ExpandParameters(selector);
                    }

                    // TODO: Find a suitable place to perform lifting of subqueries out into an OUTER APPLY or LEFT JOIN.
                    if (selector.ContainsAggregateOrSubquery()
                        || !selector.Type.IsScalarType()
                        || !translatabilityAnalyzer.CanTranslate(selector))
                    {
                        break;
                    }

                    if (node.Method.Name == nameof(Queryable.Average))
                    {
                        return new SqlAggregateExpression(
                            "AVG",
                            new SqlCastExpression(selector, node.Method.ReturnType),
                            node.Method.ReturnType,
                            relationalGrouping.IsDistinct && node.Arguments.Count == 1);
                    }
                    else if (node.Method.Name == nameof(Queryable.Sum))
                    {
                        var aggregateExpression
                            = Expression.Coalesce(
                                new SqlAggregateExpression(
                                    "SUM",
                                    selector,
                                    node.Method.ReturnType.AsNullableType(),
                                    relationalGrouping.IsDistinct && node.Arguments.Count == 1),
                                Expression.Constant(
                                    Activator.CreateInstance(node.Method.ReturnType.UnwrapNullableType())));

                        if (node.Method.ReturnType.IsNullableType())
                        {
                            return Expression.Convert(aggregateExpression, node.Method.ReturnType);
                        }
                        else
                        {
                            return aggregateExpression;
                        }
                    }
                    else
                    {
                        return new SqlAggregateExpression(
                            node.Method.Name.ToUpperInvariant(),
                            selector,
                            node.Method.ReturnType,
                            relationalGrouping.IsDistinct && node.Arguments.Count == 1);
                    }
                }

                case nameof(Queryable.Count):
                case nameof(Queryable.LongCount):
                {
                    var selector = relationalGrouping.ElementSelector;

                    if (node.Arguments.Count == 2)
                    {
                        var predicate = arguments[1].UnwrapLambda().ExpandParameters(selector);

                        if (!translatabilityAnalyzer.CanTranslate(predicate))
                        {
                            break;
                        }

                        selector
                            = Expression.Condition(
                                predicate,
                                Expression.Constant(1, typeof(int?)),
                                Expression.Constant(null, typeof(int?)));
                    }

                    var isDistinct = relationalGrouping.IsDistinct && node.Arguments.Count == 1;

                    var innerExpression =
                        (selector.Type.IsScalarType() && isDistinct) || node.Arguments.Count == 2
                            ? selector
                            : new SqlFragmentExpression("*", selector.Type);

                    return new SqlAggregateExpression(
                        "COUNT",
                        innerExpression,
                        node.Method.ReturnType,
                        isDistinct,
                        hasOpaqueType: true);
                }

                case nameof(Queryable.Distinct):
                {
                    return new GroupByResultExpression(
                        relationalGrouping.SelectExpression,
                        relationalGrouping.OuterKeySelector,
                        relationalGrouping.InnerKeySelector,
                        relationalGrouping.InnerKeyLambda,
                        relationalGrouping.ElementSelector,
                        true);
                }

                case nameof(Queryable.Select):
                {
                    var selectorLambda = arguments[1].UnwrapLambda();

                    if (selectorLambda is null || selectorLambda.Parameters.Count == 2)
                    {
                        // index parameter not supported
                        break;
                    }

                    var selectorBody
                        = selectorLambda
                            .ExpandParameters(relationalGrouping.ElementSelector);

                    if (!translatabilityAnalyzer.CanTranslate(selectorBody))
                    {
                        break;
                    }

                    return new GroupByResultExpression(
                        relationalGrouping.SelectExpression,
                        relationalGrouping.OuterKeySelector,
                        relationalGrouping.InnerKeySelector,
                        relationalGrouping.InnerKeyLambda,
                        selectorBody,
                        relationalGrouping.IsDistinct);
                }
            }
        }

        return node.Update(@object, arguments);
    }
}
