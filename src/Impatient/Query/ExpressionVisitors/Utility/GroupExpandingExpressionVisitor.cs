using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// An <see cref="ExpressionVisitor"/> that finds instances of 
    /// <see cref="GroupByResultExpression"/> and <see cref="GroupedRelationalQueryExpression"/>
    /// and replaces them with an expanded form that can be compiled.
    /// </summary>
    public class GroupExpandingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo toListGenericMethodInfo
            = GetGenericMethodDefinition((IEnumerable<object> s) => s.ToList());

        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor;
        private readonly IEnumerable<ExpressionVisitor> postExpansionVisitors;

        public GroupExpandingExpressionVisitor(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor,
            IEnumerable<ExpressionVisitor> postExpansionVisitors)
        {
            this.translatabilityAnalyzingExpressionVisitor = translatabilityAnalyzingExpressionVisitor;
            this.postExpansionVisitors = postExpansionVisitors;
        }

        private bool IsTranslatable(Expression node) => translatabilityAnalyzingExpressionVisitor.Visit(node) is TranslatableExpression;

        public override Expression Visit(Expression node)
        {
            var visited = base.Visit(node);

            switch (visited)
            {
                case GroupByResultExpression groupByResultExpression:
                {
                    var uniquifier = new TableUniquifyingExpressionVisitor();

                    var oldSelectExpression = groupByResultExpression.SelectExpression;
                    var newSelectExpression = uniquifier.VisitAndConvert(oldSelectExpression, nameof(VisitMethodCall));

                    var oldTables = oldSelectExpression.Table.Flatten().ToArray();
                    var newTables = newSelectExpression.Table.Flatten().ToArray();

                    var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                    var outerKeySelector = groupByResultExpression.OuterKeySelector;
                    var innerKeySelector = updater.Visit(groupByResultExpression.InnerKeySelector);
                    var elementSelector = updater.Visit(groupByResultExpression.ElementSelector);

                    if (IsTranslatable(outerKeySelector) && IsTranslatable(innerKeySelector) && IsTranslatable(elementSelector))
                    {
                        var projection 
                            = elementSelector
                                .VisitWith(postExpansionVisitors);

                        var predicate
                            = Expression
                                .Equal(outerKeySelector, innerKeySelector)
                                .VisitWith(postExpansionVisitors);

                        var query
                            = new EnumerableRelationalQueryExpression(
                                newSelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(projection))
                                    .AddToPredicate(predicate));

                        return ExpandedGrouping.Create(
                            visited, 
                            groupByResultExpression.OuterKeySelector,
                            query.AsList());
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                case GroupedRelationalQueryExpression groupedRelationalQueryExpression:
                {
                    var outerKeySelector = groupedRelationalQueryExpression.OuterKeySelector;
                    var innerKeySelector = groupedRelationalQueryExpression.InnerKeySelector;

                    if (IsTranslatable(outerKeySelector) && IsTranslatable(innerKeySelector))
                    {
                        if (groupedRelationalQueryExpression.RequiresDenullification)
                        {
                            outerKeySelector = JoinKeyDenullifyingExpressionVisitor.Instance.Visit(outerKeySelector);
                            innerKeySelector = JoinKeyDenullifyingExpressionVisitor.Instance.Visit(innerKeySelector);
                        }

                        var predicate
                            = Expression
                                .Equal(outerKeySelector, innerKeySelector)
                                .VisitWith(postExpansionVisitors);

                        var query
                            = new EnumerableRelationalQueryExpression(
                                groupedRelationalQueryExpression.SelectExpression
                                    .AddToPredicate(predicate));

                        return ExpandedGrouping.Create(
                            visited, 
                            groupedRelationalQueryExpression.OuterKeySelector,
                            query.AsList());
                    }
                    else
                    {
                        var predicate
                            = Expression.Lambda(
                                Expression.Equal(
                                    outerKeySelector,
                                    groupedRelationalQueryExpression.InnerKeyLambda.Body),
                                groupedRelationalQueryExpression.InnerKeyLambda.Parameters);

                        return Expression.Call(
                            queryableWhere.MakeGenericMethod(groupedRelationalQueryExpression.Type.GetSequenceType()),
                            new EnumerableRelationalQueryExpression(groupedRelationalQueryExpression.SelectExpression),
                            Expression.Quote(predicate));
                    }
                }

                default:
                {
                    return visited;
                }
            }
        }

        private static readonly MethodInfo enumerableSelect
            = GetGenericMethodDefinition((IEnumerable<object> e) => e.Select(x => x));

        private static readonly MethodInfo enumerableWhere
            = GetGenericMethodDefinition((IEnumerable<bool> e) => e.Where(x => x));

        private static readonly MethodInfo queryableSelect
            = GetGenericMethodDefinition((IQueryable<object> e) => e.Select(x => x));

        private static readonly MethodInfo queryableWhere
            = GetGenericMethodDefinition((IQueryable<bool> e) => e.Where(x => x));
    }
}
