using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.ImpatientExtensions;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class SelectorMergingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments).ToArray();

            if (node.Method.MatchesGenericMethod(selectWithoutIndex)
                && arguments[0] is MethodCallExpression previousMethodCall
                && previousMethodCall.Method.DeclaringType == typeof(Queryable))
            {
                MethodCallExpression MergeSelector(int index)
                {
                    var typeArguments = previousMethodCall.Method.GetGenericArguments();
                    var previousArguments = previousMethodCall.Arguments.ToArray();
                    var previousSelector = previousArguments[index].UnwrapLambda();
                    var currentSelector = arguments[1].UnwrapLambda();

                    previousArguments[index]
                        = Expression.Quote(
                            Expression.Lambda(
                                currentSelector.ExpandParameters(previousSelector.Body),
                                previousSelector.Parameters));

                    typeArguments[typeArguments.Length - 1] = currentSelector.ReturnType;

                    return Expression.Call(
                        previousMethodCall.Method
                            .GetGenericMethodDefinition()
                            .MakeGenericMethod(typeArguments),
                        previousArguments);
                }

                switch (previousMethodCall.Method.Name)
                {
                    case nameof(Queryable.GroupBy):
                    {
                        if (previousMethodCall.Method.MatchesGenericMethod(groupByKeyResult)
                            || previousMethodCall.Method.MatchesGenericMethod(groupByKeyElementResult))
                        {
                            var resultSelectorIndex 
                                = previousMethodCall.Method.GetParameters().ToList()
                                    .FindIndex(p => p.Name == "resultSelector");

                            var typeArguments = previousMethodCall.Method.GetGenericArguments();
                            var previousArguments = previousMethodCall.Arguments.ToArray();
                            var previousSelector = previousArguments[resultSelectorIndex].UnwrapLambda();
                            var currentSelector = arguments[1].UnwrapLambda();

                            var resultSelectorVisitor
                                = new GroupByResultSelectorExpandingExpressionVisitor(
                                    groupingParameter: null,
                                    keyParameter: previousSelector.Parameters[0],
                                    elementsParameter: previousSelector.Parameters[1]);

                            previousArguments[resultSelectorIndex]
                                = Expression.Quote(
                                    Expression.Lambda(
                                        resultSelectorVisitor.Visit(
                                            currentSelector.ExpandParameters(previousSelector.Body)),
                                        previousSelector.Parameters));

                            typeArguments[typeArguments.Length - 1] = currentSelector.ReturnType;

                            return Expression.Call(
                                previousMethodCall.Method
                                    .GetGenericMethodDefinition()
                                    .MakeGenericMethod(typeArguments),
                                previousArguments);
                        }
                        else
                        {
                            var genericArguments = previousMethodCall.Method.GetGenericArguments().ToList();
                            var genericGenericArguments = previousMethodCall.Method.GetGenericMethodDefinition().GetGenericArguments().ToList();

                            var keyTypeIndex = genericGenericArguments.FindLastIndex(a => a.Name == "TKey");
                            var keyType = genericArguments[keyTypeIndex];
                            var keyParameter = Expression.Parameter(keyType, "k");

                            var elementTypeIndex = genericGenericArguments.FindLastIndex(a => a.Name == "TElement" || a.Name == "TSource");
                            var elementType = genericArguments[elementTypeIndex];
                            var elementsParameter = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(elementType), "e");

                            var currentSelector = arguments[1].UnwrapLambda();
                            var groupingParameter = currentSelector.Parameters[0];

                            var resultSelectorVisitor 
                                = new GroupByResultSelectorExpandingExpressionVisitor(
                                    groupingParameter,
                                    keyParameter,
                                    elementsParameter);

                            var newArguments = previousMethodCall.Arguments.ToList();

                            var resultSelector
                                = Expression.Quote(
                                    Expression.Lambda(
                                        typeof(Func<,,>).MakeGenericType(
                                            keyParameter.Type, 
                                            elementsParameter.Type,
                                            currentSelector.ReturnType),
                                        resultSelectorVisitor.Visit(currentSelector.Body),
                                        keyParameter,
                                        elementsParameter));

                            genericArguments.Add(currentSelector.ReturnType);

                            var callInfo
                                = previousMethodCall.Method.MatchesGenericMethod(groupByKeyElement)
                                    ? (argumentIndex: 3, methodInfo: groupByKeyElementResult)
                                    : (argumentIndex: 2, methodInfo: groupByKeyResult);

                            newArguments.Insert(callInfo.argumentIndex, resultSelector);

                            return Expression.Call(callInfo.methodInfo.MakeGenericMethod(genericArguments.ToArray()), newArguments);
                        }
                    }

                    case nameof(Queryable.GroupJoin):
                    {
                        return MergeSelector(4);
                    }

                    case nameof(Queryable.Join):
                    {
                        return MergeSelector(4);
                    }

                    case nameof(Queryable.Select):
                    {
                        return MergeSelector(1);
                    }

                    case nameof(Queryable.SelectMany)
                    when previousMethodCall.Arguments.Count == 3:
                    {
                        return MergeSelector(2);
                    }

                    case nameof(Queryable.Zip):
                    {
                        return MergeSelector(2);
                    }
                }
            }

            return node.Update(@object, arguments);
        }

        private class GroupByResultSelectorExpandingExpressionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression groupingParameter;
            private readonly ParameterExpression keyParameter;
            private readonly ParameterExpression elementsParameter;

            public GroupByResultSelectorExpandingExpressionVisitor(
                ParameterExpression groupingParameter,
                ParameterExpression keyParameter,
                ParameterExpression elementsParameter)
            {
                this.groupingParameter = groupingParameter;
                this.keyParameter = keyParameter;
                this.elementsParameter = elementsParameter;
            }

            private static bool IsExpandedGrouping(Expression node)
            {
                return node is NewExpression newExpression 
                    && newExpression.Type.GetGenericTypeDefinition() == typeof(Grouping<,>);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member.Name == "Key" 
                    && (node.Expression == groupingParameter 
                        || IsExpandedGrouping(node.Expression)))
                {
                    return keyParameter;
                }

                return base.VisitMember(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var arguments = new Expression[node.Arguments.Count];
                var parameters = node.Method.GetParameters();

                for (var i = 0; i < arguments.Length; i++)
                {
                    var argument = node.Arguments[i];

                    if (argument == groupingParameter || IsExpandedGrouping(argument))
                    {
                        var parameter = parameters[i];

                        if (parameter.ParameterType.GetGenericTypeDefinition() != typeof(Grouping<,>))
                        {
                            arguments[i] = elementsParameter;
                            continue;
                        }
                    }

                    arguments[i] = Visit(argument);
                }

                return node.Update(Visit(node.Object), arguments);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == groupingParameter)
                {
                    var groupingType 
                        = typeof(Grouping<,>)
                            .MakeGenericType(groupingParameter.Type.GenericTypeArguments);

                    return Expression.New(
                        groupingType.GetTypeInfo().DeclaredConstructors.Single(), 
                        new[] 
                        {
                            keyParameter,
                            elementsParameter
                        },
                        new[]
                        {
                            groupingType.GetRuntimeProperty("Key"),
                            groupingType.GetRuntimeProperty("Elements"),
                        });
                }

                return base.VisitParameter(node);
            }

            private class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
            {
                public Grouping(TKey key, IEnumerable<TElement> elements)
                {
                    Key = key; // Grouping keys can be null.
                    Elements = elements ?? throw new ArgumentNullException(nameof(elements));
                }

                public TKey Key { get; }

                public IEnumerable<TElement> Elements { get; }

                public IEnumerator<TElement> GetEnumerator() => Elements.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();
            }
        }

        private static readonly MethodInfo selectWithoutIndex
            = GetGenericMethodDefinition((IQueryable<object> q) => q.Select(x => x));

        private static readonly MethodInfo groupByKey
            = GetGenericMethodDefinition((IQueryable<object> q) => q.GroupBy(x => x));

        private static readonly MethodInfo groupByKeyElement
            = GetGenericMethodDefinition((IQueryable<object> q) => q.GroupBy(x => x, x => x));

        private static readonly MethodInfo groupByKeyResult
            = GetGenericMethodDefinition((IQueryable<object> q) => q.GroupBy(x => x, (x, y) => x));

        private static readonly MethodInfo groupByKeyElementResult
            = GetGenericMethodDefinition((IQueryable<object> q) => q.GroupBy(x => x, x => x, (x, y) => x));
    }
}
