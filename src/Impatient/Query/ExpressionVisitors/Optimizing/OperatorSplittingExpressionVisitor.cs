using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.ImpatientExtensions;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    // TODO: Cover the same optimizations for the Enumerable method equivalents
    public class OperatorSplittingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (node.Method.IsGenericMethod)
            {
                var genericMethodDefinition = node.Method.GetGenericMethodDefinition();
                var genericArguments = node.Method.GetGenericArguments();

                if (predicateMethods.TryGetValue(genericMethodDefinition, out var predicateless))
                {
                    return Expression.Call(
                        predicateless.MakeGenericMethod(genericArguments),
                        Expression.Call(
                            where.MakeGenericMethod(genericArguments), 
                            arguments[0], 
                            arguments[1]));
                }
                
                if (selectorMethods.TryGetValue(genericMethodDefinition, out var selectorless))
                {
                    return Expression.Call(
                        selectorless.IsGenericMethod
                            ? selectorless.MakeGenericMethod(genericArguments[1])
                            : selectorless,
                        Expression.Call(
                            select.MakeGenericMethod(arguments[1].UnwrapLambda().Type.GenericTypeArguments),
                            arguments[0],
                            arguments[1]));
                }
            }

            return node.Update(@object, arguments);
        }

        private static readonly MethodInfo select
            = GetGenericMethodDefinition((IQueryable<object> e) => e.Select(x => x));

        private static readonly MethodInfo where
            = GetGenericMethodDefinition((IQueryable<object> e) => e.Where(x => true));

        private static readonly Dictionary<MethodInfo, MethodInfo> selectorMethods = new Dictionary<MethodInfo, MethodInfo>
        {
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Average(x => default(decimal))),
                GetMethodDefinition((IQueryable<decimal> q) => q.Average())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Average(x => default(double))),
                GetMethodDefinition((IQueryable<double> q) => q.Average())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Average(x => default(int))),
                GetMethodDefinition((IQueryable<int> q) => q.Average())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Average(x => default(long))),
                GetMethodDefinition((IQueryable<long> q) => q.Average())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Average(x => default(float))),
                GetMethodDefinition((IQueryable<float> q) => q.Average())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Average(x => default(decimal?))),
                GetMethodDefinition((IQueryable<decimal?> q) => q.Average())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Average(x => default(double?))),
                GetMethodDefinition((IQueryable<double?> q) => q.Average())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Average(x => default(int?))),
                GetMethodDefinition((IQueryable<int?> q) => q.Average())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Average(x => default(long?))),
                GetMethodDefinition((IQueryable<long?> q) => q.Average())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Average(x => default(float?))),
                GetMethodDefinition((IQueryable<float?> q) => q.Average())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Max(x => x)),
                GetGenericMethodDefinition((IQueryable<decimal> q) => q.Max())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Min(x => x)),
                GetGenericMethodDefinition((IQueryable<decimal> q) => q.Min())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Sum(x => default(decimal))),
                GetMethodDefinition((IQueryable<decimal> q) => q.Sum())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Sum(x => default(double))),
                GetMethodDefinition((IQueryable<double> q) => q.Sum())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Sum(x => default(int))),
                GetMethodDefinition((IQueryable<int> q) => q.Sum())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Sum(x => default(long))),
                GetMethodDefinition((IQueryable<long> q) => q.Sum())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Sum(x => default(float))),
                GetMethodDefinition((IQueryable<float> q) => q.Sum())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Sum(x => default(decimal?))),
                GetMethodDefinition((IQueryable<decimal?> q) => q.Sum())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Sum(x => default(double?))),
                GetMethodDefinition((IQueryable<double?> q) => q.Sum())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Sum(x => default(int?))),
                GetMethodDefinition((IQueryable<int?> q) => q.Sum())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Sum(x => default(long?))),
                GetMethodDefinition((IQueryable<long?> q) => q.Sum())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Sum(x => default(float?))),
                GetMethodDefinition((IQueryable<float?> q) => q.Sum())
            },
        };

        private static readonly Dictionary<MethodInfo, MethodInfo> predicateMethods = new Dictionary<MethodInfo, MethodInfo>
        {
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Count(x => true)),
                GetGenericMethodDefinition((IQueryable<object> q) => q.Count())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.First(x => true)),
                GetGenericMethodDefinition((IQueryable<object> q) => q.First())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.FirstOrDefault(x => true)),
                GetGenericMethodDefinition((IQueryable<object> q) => q.FirstOrDefault())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Last(x => true)),
                GetGenericMethodDefinition((IQueryable<object> q) => q.Last())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.LastOrDefault(x => true)),
                GetGenericMethodDefinition((IQueryable<object> q) => q.LastOrDefault())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.LongCount(x => true)),
                GetGenericMethodDefinition((IQueryable<object> q) => q.LongCount())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.Single(x => true)),
                GetGenericMethodDefinition((IQueryable<object> q) => q.Single())
            },
            {
                GetGenericMethodDefinition((IQueryable<object> q) => q.SingleOrDefault(x => true)),
                GetGenericMethodDefinition((IQueryable<object> q) => q.SingleOrDefault())
            },
        };
    }
}
