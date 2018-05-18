using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class GroupingAggregationRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor;

        public GroupingAggregationRewritingExpressionVisitor(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor)
        {
            this.translatabilityAnalyzingExpressionVisitor
                = translatabilityAnalyzingExpressionVisitor
                    ?? throw new ArgumentNullException(nameof(translatabilityAnalyzingExpressionVisitor));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (node.Method.IsQueryableOrEnumerableMethod()
                && !node.ContainsNonLambdaDelegates()
                && arguments[0] is GroupByResultExpression relationalGrouping)
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

                        if (!(translatabilityAnalyzingExpressionVisitor.Visit(selector) is TranslatableExpression))
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

                        return new SqlAggregateExpression(
                            node.Method.Name.ToUpperInvariant(),
                            selector,
                            node.Method.ReturnType,
                            relationalGrouping.IsDistinct && node.Arguments.Count == 1);
                    }

                    case nameof(Queryable.Count):
                    case nameof(Queryable.LongCount):
                    {
                        var selector = relationalGrouping.ElementSelector;

                        if (node.Arguments.Count == 2)
                        {
                            var predicate = node.Arguments[1].UnwrapLambda().ExpandParameters(selector);

                            if (!(translatabilityAnalyzingExpressionVisitor.Visit(predicate) is TranslatableExpression))
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
                            "COUNT",
                            selector.Type.IsScalarType() ? selector : new SqlFragmentExpression("*", selector.Type),
                            node.Method.ReturnType,
                            relationalGrouping.IsDistinct && node.Arguments.Count == 1);
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
                        var selectorLambda = node.Arguments[1].UnwrapLambda();

                        if (selectorLambda == null || selectorLambda.Parameters.Count == 2)
                        {
                            // index parameter not supported
                            break;
                        }

                        var selectorBody
                            = selectorLambda
                                .ExpandParameters(relationalGrouping.ElementSelector);

                        if (!(translatabilityAnalyzingExpressionVisitor.Visit(selectorBody) is TranslatableExpression))
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
}
