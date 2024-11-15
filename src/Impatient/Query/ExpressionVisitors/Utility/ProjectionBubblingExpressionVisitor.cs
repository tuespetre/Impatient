using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility;

public class ProjectionBubblingExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        return node;
    }

    protected override Expression VisitExtension(Expression node)
    {
        switch (node)
        {
            case EnumerableRelationalQueryExpression query:
            {
                return query.SelectExpression.Projection;
            }

            default:
            {
                return base.VisitExtension(node);
            }
        }
    }

    protected override Expression VisitNew(NewExpression node)
    {
        if (node.Members is not null && node.Type.IsGenericType(typeof(ExpandedGrouping<,>)))
        {
            return Visit(node.Arguments[1]);
        }

        return node;
    }

    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.IsQueryableOrEnumerableMethod()
            && node.Arguments.Count > 0
            && !node.ContainsNonLambdaExpressions()
            && !node.ContainsNonLambdaDelegates())
        {
            var arguments = Visit(node.Arguments);

            switch (node.Method.Name)
            {
                case nameof(Queryable.Select):
                {
                    if (arguments[0] is ProjectionExpression projection)
                    {
                        return MergeBubbledProjection(projection, arguments[1].UnwrapLambda());
                    }

                    return node;
                }

                case nameof(Queryable.SelectMany):
                {
                    if (arguments[0] is ProjectionExpression outerProjection)
                    {
                        var innerProjection = arguments[1] as ProjectionExpression;

                        if (innerProjection is null)
                        {
                            var collectionSelectorLambda = arguments[1].UnwrapLambda();

                            if (collectionSelectorLambda is not null)
                            {
                                var expanded
                                    = collectionSelectorLambda
                                        .ExpandParameters(outerProjection.Flatten().Body);

                                innerProjection = Visit(expanded) as ProjectionExpression;
                            }
                        }

                        if (innerProjection is not null)
                        {
                            if (arguments.Count == 2)
                            {
                                return innerProjection;
                            }
                            else
                            {
                                return new CompositeProjectionExpression(
                                    outerProjection,
                                    innerProjection,
                                    arguments[2].UnwrapLambda());
                            }
                        }
                    }

                    return node;
                }

                case nameof(Queryable.Join):
                {
                    if (arguments[0] is ProjectionExpression outerProjection
                        && arguments[1] is ProjectionExpression innerProjection)
                    {
                        return new CompositeProjectionExpression(
                            outerProjection,
                            innerProjection,
                            arguments[4].UnwrapLambda());
                    }

                    return node;
                }

                case nameof(Queryable.GroupJoin):
                {
                    if (arguments[0] is ProjectionExpression outerProjection
                        && arguments[1] is ProjectionExpression innerProjection)
                    {
                        return new CompositeProjectionExpression(
                            outerProjection,
                            new ServerProjectionExpression(
                                new SurrogateEnumerableRelationalQueryExpression(
                                    new SelectExpression(innerProjection))),
                            arguments[4].UnwrapLambda());
                    }

                    return node;
                }

                case nameof(Queryable.GroupBy):
                {
                    if (arguments[0] is ProjectionExpression projection
                        && node.Method.HasResultSelector())
                    {
                        var keyExpression
                            = MergeBubbledProjection(projection, arguments[1].UnwrapLambda()).Flatten().Body;

                        var elementProjectionBody
                            = Visit(projection.Flatten().Body);

                        if (node.Method.HasElementSelector())
                        {
                            elementProjectionBody
                                = MergeBubbledProjection(projection, arguments[2].UnwrapLambda()).Flatten().Body;
                        }

                        var elementExpression
                            = new SurrogateEnumerableRelationalQueryExpression(
                                new SelectExpression(
                                    new ServerProjectionExpression(
                                        elementProjectionBody)));

                        //if (node.Method.HasResultSelector())
                        {
                            var resultSelector
                                = node.Method.HasElementSelector()
                                    ? arguments[3]
                                    : arguments[2];

                            return new ServerProjectionExpression(
                                resultSelector
                                    .UnwrapLambda()
                                    .ExpandParameters(keyExpression, elementExpression));
                        }

                        /*return new ServerProjectionExpression(
                            ExpandedGrouping.Create(
                                node.Type.GetSequenceType(),
                                keyExpression,
                                elementExpression.AsList()));*/
                    }

                    return node;
                }

                case nameof(Queryable.Zip):
                {
                    if (arguments[0] is ProjectionExpression outerProjection
                        && arguments[1] is ProjectionExpression innerProjection)
                    {
                        return new CompositeProjectionExpression(
                            outerProjection,
                            innerProjection,
                            arguments[2].UnwrapLambda());
                    }

                    return node;
                }

                case nameof(Queryable.All):
                case nameof(Queryable.Any):
                case nameof(Queryable.Average):
                case nameof(Queryable.Contains):
                case nameof(Queryable.Count):
                case nameof(Queryable.LongCount):
                case nameof(Queryable.Max) when node.Type.IsScalarType():
                case nameof(Queryable.MaxBy) when node.Type.IsScalarType():
                case nameof(Queryable.Min) when node.Type.IsScalarType():
                case nameof(Queryable.MinBy) when node.Type.IsScalarType():
                case nameof(Queryable.SequenceEqual):
                case nameof(Queryable.Sum):
                {
                    return new ServerProjectionExpression(Expression.Default(node.Type));
                }

                case nameof(Queryable.AsQueryable):
                case nameof(Enumerable.AsEnumerable):

                case nameof(Queryable.ElementAt):
                case nameof(Queryable.ElementAtOrDefault):
                case nameof(Queryable.First):
                case nameof(Queryable.FirstOrDefault):
                case nameof(Queryable.Last):
                case nameof(Queryable.LastOrDefault):
                case nameof(Queryable.Single):
                case nameof(Queryable.SingleOrDefault):

                case nameof(Queryable.Distinct):
                case nameof(Queryable.DistinctBy):
                case nameof(Queryable.Except):
                case nameof(Queryable.ExceptBy):
                case nameof(Queryable.Intersect):
                case nameof(Queryable.IntersectBy):
                case nameof(Queryable.Union):
                case nameof(Queryable.UnionBy):

                case nameof(Queryable.Order):
                case nameof(Queryable.OrderDescending):
                case nameof(Queryable.OrderBy):
                case nameof(Queryable.OrderByDescending):
                case nameof(Queryable.ThenBy):
                case nameof(Queryable.ThenByDescending):
                case nameof(Queryable.Reverse):

                case nameof(Queryable.Skip):
                case nameof(Queryable.SkipLast):
                case nameof(Queryable.SkipWhile):
                case nameof(Queryable.Take):
                case nameof(Queryable.TakeLast):
                case nameof(Queryable.TakeWhile):

                case nameof(Enumerable.ToArray):
                case nameof(Enumerable.ToDictionary):
                case nameof(Enumerable.ToHashSet):
                case nameof(Enumerable.ToList):
                case nameof(Enumerable.ToLookup):
                {
                    if (arguments[0] is ProjectionExpression projection)
                    {
                        return projection;
                    }

                    return node;
                }

                // for the following operators, as well as any new operators that might come along,
                // we either cannot reliably bubble a projection out of them, or we need to give them
                // more thoughtful consideration.

                case nameof(Queryable.Aggregate):

                case nameof(Queryable.Append):
                case nameof(Queryable.Concat):
                case nameof(Queryable.Prepend):

                case nameof(Queryable.Cast):
                case nameof(Queryable.OfType):

                case nameof(Queryable.DefaultIfEmpty):

                case nameof(Queryable.Chunk):
                case nameof(Enumerable.Empty):
                case nameof(Enumerable.Range):
                case nameof(Enumerable.Repeat):

                default:
                {
                    return node;
                }
            }
        }

        if (node.Method.IsAsOrderedQueryableMethod())
        {
            var arguments = Visit(node.Arguments);

            if (arguments[0] is ProjectionExpression projection)
            {
                return projection;
            }
        }

        return node;
    }

    private ProjectionExpression MergeBubbledProjection(ProjectionExpression projection, LambdaExpression lambda)
    {
        var flattened = Visit(projection.Flatten().Body);

        if (flattened is ProjectionExpression projectionFromFlattened)
        {
            return projectionFromFlattened.Merge(lambda);
        }

        return projection.Merge(lambda);
    }

    private class SurrogateEnumerableRelationalQueryExpression : EnumerableRelationalQueryExpression
    {
        public SurrogateEnumerableRelationalQueryExpression(SelectExpression selectExpression) : base(selectExpression)
        {
        }
    }

    private class SurrogateGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        public TKey Key => throw new InvalidOperationException();

        public IEnumerator<TElement> GetEnumerator() => throw new InvalidOperationException();

        IEnumerator IEnumerable.GetEnumerator() => throw new InvalidOperationException();
    }
}
