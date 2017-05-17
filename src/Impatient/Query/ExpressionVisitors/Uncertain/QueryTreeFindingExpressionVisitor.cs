using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Impatient.Query.ExpressionVisitors
{
    public class QueryTreeFindingExpressionVisitor : ExpressionVisitor
    {
        private HashSet<MethodCallExpression> visitedMethodCalls = new HashSet<MethodCallExpression>();

        public IDictionary<MethodCallExpression, IEnumerable<IEnumerable<MethodCallExpression>>> Trees { get; } 
            = new Dictionary<MethodCallExpression, IEnumerable<IEnumerable<MethodCallExpression>>>();

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) && !visitedMethodCalls.Contains(node))
            {
                var paths = IterateQueryPaths(node);

                visitedMethodCalls = new HashSet<MethodCallExpression>(paths.SelectMany(path => path).Concat(visitedMethodCalls));

                Trees.Add(node, paths.Select(path => path.ToList()).ToList());
            }

            return base.VisitMethodCall(node);
        }

        private static IEnumerable<IEnumerable<MethodCallExpression>> IterateQueryPaths(Expression expression)
        {
            if (expression is MethodCallExpression methodCall
                && methodCall.Method.DeclaringType == typeof(Queryable)) //!methodCall.Method.GetQueryOperator().Equals(QueryOperator.None))
            {
                return IterateQueryPaths(methodCall);
            }

            return Enumerable.Empty<IEnumerable<MethodCallExpression>>();
        }

        private static IEnumerable<IEnumerable<MethodCallExpression>> IterateQueryPaths(MethodCallExpression methodCallExpression)
        {
            var current = Enumerable.Repeat(methodCallExpression, 1);
            var yielded = false;

            if (methodCallExpression.Arguments[0] is MethodCallExpression outer
                && outer.Method.DeclaringType == typeof(Queryable))
            {
                foreach (var path in IterateQueryPaths(outer))
                {
                    yielded |= true;
                    yield return current.Concat(path);
                }
            }

            switch (methodCallExpression.Method.Name)
            {
                case nameof(Queryable.Concat):
                case nameof(Queryable.GroupJoin):
                case nameof(Queryable.Intersect):
                case nameof(Queryable.Join):
                case nameof(Queryable.SequenceEqual):
                case nameof(Queryable.Union):
                case nameof(Queryable.Zip):
                {
                    if (methodCallExpression.Arguments[1] is MethodCallExpression inner
                        && inner.Method.DeclaringType == typeof(Queryable))
                    {
                        foreach (var path in IterateQueryPaths(inner))
                        {
                            yielded |= true;
                            yield return current.Concat(path);
                        }
                    }

                    break;
                }

                case nameof(Queryable.SelectMany):
                {
                    if (methodCallExpression.Arguments[1].UnwrapLambda().Body is MethodCallExpression inner
                        && inner.Method.DeclaringType == typeof(Queryable))
                    {
                        foreach (var path in IterateQueryPaths(inner))
                        {
                            yielded |= true;
                            yield return current.Concat(path);
                        }
                    }

                    break;
                }
            }

            if (!yielded)
            {
                yield return current;
            }
        }

    }
}
