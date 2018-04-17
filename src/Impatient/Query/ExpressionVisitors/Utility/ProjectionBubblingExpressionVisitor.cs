using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
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
            if (node.Members != null && node.Type.IsGenericType(typeof(ExpandedGrouping<,>)))
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
                            return projection.Merge(arguments[1].UnwrapLambda());
                        }

                        return node;
                    }

                    case nameof(Queryable.SelectMany):
                    {
                        if (arguments[0] is ProjectionExpression outerProjection)
                        {
                            var innerProjection = arguments[1] as ProjectionExpression;

                            if (innerProjection == null)
                            {
                                var collectionSelectorLambda = arguments[1].UnwrapLambda();

                                if (collectionSelectorLambda != null)
                                {
                                    var expanded
                                        = collectionSelectorLambda
                                            .ExpandParameters(outerProjection.Flatten().Body);

                                    innerProjection = Visit(expanded) as ProjectionExpression;
                                }
                            }

                            if (innerProjection != null)
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
                        if (arguments[0] is ProjectionExpression projection)
                        {
                            var keyExpression = projection.Merge(arguments[1].UnwrapLambda()).Flatten().Body;

                            var elementProjectionBody
                                = projection.Flatten().Body;

                            if (node.Method.HasElementSelector())
                            {
                                elementProjectionBody
                                    = projection.Merge(arguments[2].UnwrapLambda()).Flatten().Body;
                            }

                            var elementExpression
                                = new SurrogateEnumerableRelationalQueryExpression(
                                    new SelectExpression(
                                        new ServerProjectionExpression(
                                            elementProjectionBody)));

                            if (node.Method.HasResultSelector())
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

                            return new ServerProjectionExpression(
                                ExpandedGrouping.Create(
                                    node,
                                    keyExpression,
                                    elementExpression));
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
                    case nameof(Queryable.Contains):
                    case nameof(Queryable.Count):
                    case nameof(Queryable.LongCount):
                    case nameof(Queryable.Max):
                    case nameof(Queryable.Min):
                    case nameof(Queryable.Average):
                    case nameof(Queryable.Sum):
                    {
                        return new ServerProjectionExpression(Expression.Default(node.Type));
                    }

                    case nameof(Enumerable.ToArray):
                    case nameof(Enumerable.ToDictionary):
                    //case nameof(Enumerable.ToHashSet):
                    case nameof(Enumerable.ToList):
                    case nameof(Enumerable.ToLookup):
                    default:
                    {
                        if (arguments[0] is ProjectionExpression projection)
                        {
                            return projection;
                        }

                        return node;
                    }
                }
            }

            return node;
        }

        private class SurrogateEnumerableRelationalQueryExpression : EnumerableRelationalQueryExpression
        {
            public SurrogateEnumerableRelationalQueryExpression(SelectExpression selectExpression) : base(selectExpression)
            {
            }
        }
    }
}
