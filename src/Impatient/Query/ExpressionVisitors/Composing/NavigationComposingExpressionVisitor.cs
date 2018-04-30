using Impatient.Extensions;
using Impatient.Metadata;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Composing
{
    public class NavigationComposingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo queryableSelectMethodInfo
            = GetGenericMethodDefinition((IQueryable<object> o) => o.Select(x => x));

        private static readonly MethodInfo enumerableSelectMethodInfo
            = GetGenericMethodDefinition((IEnumerable<object> o) => o.Select(x => x));

        private static readonly MethodInfo queryableWhereMethodInfo
            = GetGenericMethodDefinition((IQueryable<bool> o) => o.Where(x => x));

        private static readonly MethodInfo enumerableWhereMethodInfo
            = GetGenericMethodDefinition((IEnumerable<bool> o) => o.Where(x => x));

        private static readonly MethodInfo enumerableCountMethodInfo
            = GetGenericMethodDefinition<IEnumerable<object>, int>(o => o.Count());

        private readonly IEnumerable<NavigationDescriptor> navigationDescriptors;

        public NavigationComposingExpressionVisitor(IEnumerable<NavigationDescriptor> navigationDescriptors)
        {
            this.navigationDescriptors = navigationDescriptors ?? throw new ArgumentNullException(nameof(navigationDescriptors));
        }

        public override Expression Visit(Expression node)
        {
            node = new CoreNavigationRewritingExpressionVisitor(navigationDescriptors).Visit(node);
            node = new NavigationContextTerminatingExpressionVisitor().Visit(node);

            return node;
        }

        private sealed class CoreNavigationRewritingExpressionVisitor : ExpressionVisitor
        {
            private readonly IEnumerable<NavigationDescriptor> navigationDescriptors;

            public CoreNavigationRewritingExpressionVisitor(IEnumerable<NavigationDescriptor> navigationDescriptors)
            {
                this.navigationDescriptors = navigationDescriptors ?? throw new ArgumentNullException(nameof(navigationDescriptors));
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (!node.Method.IsQueryableOrEnumerableMethod()
                    || node.ContainsNonLambdaDelegates()
                    || node.ContainsNonLambdaExpressions())
                {
                    return base.VisitMethodCall(node);
                }
                
                switch (node.Method.Name)
                {
                    // Uncertain
                    case nameof(Queryable.Aggregate):
                    {
                        break;
                    }

                    case nameof(Enumerable.ToDictionary):
                    case nameof(Enumerable.ToLookup):
                    {
                        // TODO: Handle ToDictionary
                        // TODO: Handle ToLookup
                        break;
                    }

                    // Pass-through methods (just need a generic type change)
                    case nameof(Enumerable.AsEnumerable):
                    case nameof(Queryable.AsQueryable):
                    case nameof(Queryable.Cast):
                    case nameof(Queryable.OfType):
                    case nameof(Queryable.Reverse):
                    case nameof(Queryable.Skip):
                    case nameof(Queryable.Take):
                    //case nameof(Queryable.SkipLast):
                    //case nameof(Queryable.TakeLast):
                    {
                        return HandlePassthroughMethod(node);
                    }

                    // Complex selector lambdas

                    case nameof(Queryable.Select):
                    {
                        return HandleSelect(node);
                    }

                    case nameof(Queryable.SelectMany):
                    {
                        return HandleSelectMany(node);
                    }

                    case nameof(Queryable.GroupBy):
                    {
                        return HandleGroupBy(node);
                    }

                    case nameof(Queryable.GroupJoin):
                    {
                        return HandleGroupJoin(node);
                    }

                    case nameof(Queryable.Join):
                    {
                        return HandleJoin(node);
                    }

                    case nameof(Queryable.Zip):
                    {
                        return HandleZip(node);
                    }

                    // Intermediate selector lambdas
                    case nameof(Queryable.OrderBy):
                    case nameof(Queryable.OrderByDescending):
                    case nameof(Queryable.ThenBy):
                    case nameof(Queryable.ThenByDescending):
                    {
                        return HandleIntermediateSelectorMethod(node);
                    }

                    // Terminal selector lambdas
                    case nameof(Queryable.Average) when node.Arguments.Count == 2:
                    case nameof(Queryable.Max) when node.Arguments.Count == 2:
                    case nameof(Queryable.Min) when node.Arguments.Count == 2:
                    case nameof(Queryable.Sum) when node.Arguments.Count == 2:
                    {
                        return HandleTerminalSelectorMethod(node);
                    }

                    // Type-independent terminal predicate lambdas
                    case nameof(Queryable.All):
                    case nameof(Queryable.Any) when node.Arguments.Count == 2:
                    case nameof(Queryable.Count) when node.Arguments.Count == 2:
                    case nameof(Queryable.LongCount) when node.Arguments.Count == 2:
                    {
                        return HandleTypeIndependentTerminalPredicateMethod(node);
                    }

                    // Type-dependent terminal predicate lambdas
                    case nameof(Queryable.First) when node.Arguments.Count == 2:
                    case nameof(Queryable.FirstOrDefault) when node.Arguments.Count == 2:
                    case nameof(Queryable.Last) when node.Arguments.Count == 2:
                    case nameof(Queryable.LastOrDefault) when node.Arguments.Count == 2:
                    case nameof(Queryable.Single) when node.Arguments.Count == 2:
                    case nameof(Queryable.SingleOrDefault) when node.Arguments.Count == 2:
                    {
                        return HandleTypeDependentTerminalPredicateMethod(node);
                    }

                    // Non-terminal predicate lambdas
                    case nameof(Queryable.SkipWhile):
                    case nameof(Queryable.TakeWhile):
                    case nameof(Queryable.Where):
                    {
                        return HandleNonTerminalPredicateMethod(node);
                    }
                }

                return base.VisitMethodCall(node);
            }

            private MethodCallExpression CreateTerminalCall(
                MethodCallExpression originalNode,
                Expression result,
                NavigationExpansionContext context)
            {
                var queryable = originalNode.Method.IsQueryableMethod();

                var terminalSelectMethod = queryable ? queryableSelectMethodInfo : enumerableSelectMethodInfo;

                var terminalSelectorArgument = (Expression)context.OuterTerminalSelector;

                if (queryable)
                {
                    terminalSelectorArgument = Expression.Quote(terminalSelectorArgument);
                }

                return Expression.Call(
                    terminalSelectMethod.MakeGenericMethod(
                        result.Type.GetSequenceType(),
                        originalNode.Type.GetSequenceType()),
                    result,
                    terminalSelectorArgument);
            }

            private MethodCallExpression CreateCall(
                MethodInfo method,
                IEnumerable<Expression> arguments)
            {
                if (method.IsQueryableMethod())
                {
                    arguments = arguments.Select(a => a is LambdaExpression ? Expression.Quote(a) : a);
                }

                return Expression.Call(method, arguments);
            }

            private Expression HandlePassthroughMethod(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);

                if (source is NavigationExpansionContextExpression nece)
                {
                    var result
                        = CreateCall(
                            node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                nece.Source.Type.GetSequenceType()),
                            node.Arguments.Skip(1).Prepend(nece.Source));

                    return new NavigationExpansionContextExpression(
                        CreateTerminalCall(node, result, nece.Context),
                        nece.Context);
                }
                else
                {
                    return node.Update(null, node.Arguments.Skip(1).Prepend(source));
                }
            }

            private Expression HandleIntermediateSelectorMethod(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var selector = Visit(node.Arguments[1]).UnwrapLambda();
                var parameter = selector.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(parameter, navigationDescriptors);
                }

                if (context.ExpandIntermediateLambda(ref source, ref selector, ref parameter, out _))
                {
                    var result
                        = CreateCall(
                            node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                parameter.Type,
                                node.Method.GetGenericArguments()[1]),
                            new[] { source, selector });

                    return new NavigationExpansionContextExpression(
                        CreateTerminalCall(node, result, context),
                        context);
                }
                else
                {
                    return node.Update(null, new[] { source, selector }.Concat(node.Arguments.Skip(2)));
                }
            }

            private Expression HandleTerminalSelectorMethod(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var selector = Visit(node.Arguments[1]).UnwrapLambda();
                var parameter = selector.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(parameter, navigationDescriptors);
                }

                if (context.ExpandIntermediateLambda(ref source, ref selector, ref parameter, out _))
                {
                    return CreateCall(
                        node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                            node.Method.GetGenericArguments()
                                .Skip(1)
                                .Prepend(parameter.Type)
                                .ToArray()),
                        new[] { source, selector });
                }
                else
                {
                    return node.Update(null, new[] { source, selector }.Concat(node.Arguments.Skip(2)));
                }
            }

            private Expression HandleTypeIndependentTerminalPredicateMethod(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var predicate = Visit(node.Arguments[1]).UnwrapLambda();
                var parameter = predicate.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(parameter, navigationDescriptors);
                }

                if (context.ExpandIntermediateLambda(ref source, ref predicate, ref parameter, out _))
                {
                    return CreateCall(
                        node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                            parameter.Type),
                        new[] { source, predicate });
                }
                else
                {
                    return node.Update(null, new[] { source, predicate }.Concat(node.Arguments.Skip(2)));
                }
            }

            private Expression HandleTypeDependentTerminalPredicateMethod(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var predicate = Visit(node.Arguments[1]).UnwrapLambda();
                var parameter = predicate.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(parameter, navigationDescriptors);
                }

                if (context.ExpandIntermediateLambda(ref source, ref predicate, ref parameter, out _))
                {
                    var terminalSelectMethod = enumerableSelectMethodInfo;
                    var terminalWhereMethod = enumerableWhereMethodInfo;
                    var predicateArgument = (Expression)predicate;
                    var terminalSelectorArgument = (Expression)context.OuterTerminalSelector;

                    if (node.Method.IsQueryableMethod())
                    {
                        terminalSelectMethod = queryableSelectMethodInfo;
                        terminalWhereMethod = queryableWhereMethodInfo;
                        predicateArgument = Expression.Quote(predicateArgument);
                        terminalSelectorArgument = Expression.Quote(terminalSelectorArgument);
                    }

                    return Expression.Call(
                        node.Method.DeclaringType
                            .GetTypeInfo()
                            .GetDeclaredMethods(node.Method.Name)
                            .Single(m => m.GetParameters().Length == 1)
                            .MakeGenericMethod(node.Method.GetGenericArguments()[0]),
                        Expression.Call(
                            terminalSelectMethod.MakeGenericMethod(
                                parameter.Type,
                                node.Method.GetGenericArguments()[0]),
                            Expression.Call(
                                terminalWhereMethod.MakeGenericMethod(parameter.Type),
                                source,
                                predicateArgument),
                            terminalSelectorArgument));
                }
                else
                {
                    return node.Update(null, new[] { source, predicate }.Concat(node.Arguments.Skip(2)));
                }
            }

            private Expression HandleNonTerminalPredicateMethod(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var predicate = Visit(node.Arguments[1]).UnwrapLambda();
                var parameter = predicate.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(parameter, navigationDescriptors);
                }

                if (context.ExpandIntermediateLambda(ref source, ref predicate, ref parameter, out _))
                {
                    var result
                        = CreateCall(
                            node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                parameter.Type),
                            new[] { source, predicate });

                    return new NavigationExpansionContextExpression(
                        CreateTerminalCall(node, result, context),
                        context);
                }
                else
                {
                    return node.Update(null, new[] { source, predicate }.Concat(node.Arguments.Skip(2)));
                }
            }

            private Expression HandleSelect(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var selector = Visit(node.Arguments[1]).UnwrapLambda();
                var parameter = selector.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(parameter, navigationDescriptors);
                }

                if (context.ExpandResultLambda(ref source, ref selector, ref parameter))
                {
                    var result
                        = CreateCall(
                            node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                parameter.Type,
                                selector.ReturnType),
                            new[] { source, selector });

                    return new NavigationExpansionContextExpression(
                        CreateTerminalCall(node, result, context),
                        context);
                }
                else
                {
                    return node.Update(null, new[] { source, selector }.Concat(node.Arguments.Skip(2)));
                }
            }

            private Expression HandleSelectMany(MethodCallExpression node)
            {
                if (node.Arguments.Count == 2)
                {
                    return HandleSelectManyWithoutResultSelector(node);
                }
                else
                {
                    return HandleSelectManyWithResultSelector(node);
                }
            }

            private Expression HandleSelectManyWithoutResultSelector(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var selector = Visit(node.Arguments[1]).UnwrapLambda();
                var parameter = selector.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(parameter, navigationDescriptors);
                }

                if (context.ExpandIntermediateLambda(ref source, ref selector, ref parameter, out _))
                {
                    return CreateCall(
                        node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                            parameter.Type,
                            node.Method.GetGenericArguments()[1]),
                        new[] { source, selector });
                }
                else
                {
                    return node.Update(null, new[] { source, selector }.Concat(node.Arguments.Skip(2)));
                }
            }

            private Expression HandleSelectManyWithResultSelector(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var collectionSelector = Visit(node.Arguments[1]).UnwrapLambda();
                var resultSelector = Visit(node.Arguments[2]).UnwrapLambda();
                var parameter1 = collectionSelector.Parameters[0];
                var parameter2 = resultSelector.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(parameter1, navigationDescriptors);
                }

                if (context.ExpandIntermediateLambda(ref source, ref collectionSelector, ref parameter1, out _)
                    && context.ExpandResultLambda(ref source, ref resultSelector, ref parameter2))
                {
                    // TODO: The collectionSelector needs to be wrapped
                    // with a call to AsEnumerable in case it is returning IQueryable.

                    var result
                        = CreateCall(
                            node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                parameter1.Type,
                                node.Method.GetGenericArguments()[1],
                                resultSelector.ReturnType),
                            new[] { source, collectionSelector, resultSelector });

                    return new NavigationExpansionContextExpression(
                        CreateTerminalCall(node, result, context),
                        context);
                }
                else
                {
                    return node.Update(null, new[] { source, collectionSelector, resultSelector }.Concat(node.Arguments.Skip(3)));
                }
            }

            private Expression HandleGroupBy(MethodCallExpression node)
            {
                var parameters = node.Method.GetParameters();
                var hasElementSelector = parameters.Any(p => p.Name == "elementSelector");
                var hasResultSelector = parameters.Any(p => p.Name == "resultSelector");

                if (hasElementSelector && hasResultSelector)
                {
                    return HandleGroupByWithElementAndResultSelectors(node);
                }
                else if (hasElementSelector)
                {
                    return HandleGroupByWithElementSelector(node);
                }
                else if (hasResultSelector)
                {
                    return HandleGroupByWithResultSelector(node);
                }
                else
                {
                    return HandleGroupByWithoutElementAndResultSelectors(node);
                }
            }

            private Expression HandleGroupByWithElementAndResultSelectors(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var keySelector = Visit(node.Arguments[1]).UnwrapLambda();
                var elementSelector = Visit(node.Arguments[2]).UnwrapLambda();
                var resultSelector = Visit(node.Arguments[3]).UnwrapLambda();
                var keyParameter = keySelector.Parameters[0];
                var elementParameter = elementSelector.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(keyParameter, navigationDescriptors);
                }

                bool foundKeyNavigations, foundElementNavigations, expandedAny = false;

                var originalKeySelector = keySelector;
                var originalKeyParameter = keyParameter;
                var originalElementSelector = elementSelector;
                var originalElementParameter = elementParameter;

                do
                {
                    keySelector = originalKeySelector;
                    keyParameter = originalKeyParameter;
                    elementSelector = originalElementSelector;
                    elementParameter = originalElementParameter;

                    expandedAny
                        |= context.ExpandIntermediateLambda(
                            ref source,
                            ref keySelector,
                            ref keyParameter,
                            out foundKeyNavigations);

                    expandedAny
                        |= context.ExpandIntermediateLambda(
                            ref source,
                            ref elementSelector,
                            ref elementParameter,
                            out foundElementNavigations);
                }
                while (foundKeyNavigations || foundElementNavigations);

                var resultKeyParameter = resultSelector.Parameters[0];
                var resultElementsParameter = resultSelector.Parameters[1];

                var intermediateResultType
                    = typeof(NavigationTransparentIdentifier<,>).MakeGenericType(
                        resultKeyParameter.Type,
                        resultElementsParameter.Type);

                var resultOuterField = intermediateResultType.GetRuntimeField("Outer");
                var resultInnerField = intermediateResultType.GetRuntimeField("Inner");

                var intermediateResultSelector
                    = Expression.Lambda(
                        Expression.New(
                            intermediateResultType.GetTypeInfo().DeclaredConstructors.Single(),
                            new[] { resultKeyParameter, resultElementsParameter },
                            new[] { resultOuterField, resultInnerField }),
                        new[] { resultKeyParameter, resultElementsParameter });

                var newSource
                    = CreateCall(
                        node.Method
                            .GetGenericMethodDefinition()
                            .MakeGenericMethod(new[]
                            {
                                source.Type.GetSequenceType(),
                                keySelector.ReturnType,
                                elementSelector.ReturnType,
                                intermediateResultType,
                            }),
                        new[]
                        {
                            source,
                            keySelector,
                            elementSelector,
                            intermediateResultSelector,
                        }.Concat(node.Arguments.Skip(4))) as Expression;

                var newResultParameter = Expression.Parameter(intermediateResultType, "<>nav");
                var newContext = new NavigationExpansionContext(newResultParameter, navigationDescriptors);

                var newResultSelector
                    = Expression.Lambda(
                        resultSelector.Body
                            .Replace(
                                resultKeyParameter,
                                Expression.MakeMemberAccess(newResultParameter, resultOuterField))
                            .Replace(
                                resultElementsParameter,
                                Expression.MakeMemberAccess(newResultParameter, resultInnerField)),
                        newResultParameter);

                if (newContext.ExpandResultLambda(ref newSource, ref newResultSelector, ref newResultParameter))
                {
                    var terminalSelectMethod = enumerableSelectMethodInfo;
                    var terminalSelector = (Expression)newContext.OuterTerminalSelector;

                    if (node.Method.IsQueryableMethod())
                    {
                        terminalSelectMethod = queryableSelectMethodInfo;
                        terminalSelector = Expression.Quote(terminalSelector);
                    }

                    var result
                        = Expression.Call(
                            terminalSelectMethod.MakeGenericMethod(
                                newSource.Type.GetSequenceType(),
                                newResultSelector.ReturnType),
                            newSource,
                            newResultSelector);

                    var terminator
                        = Expression.Call(
                            terminalSelectMethod.MakeGenericMethod(
                                newResultSelector.ReturnType,
                                node.Type.GetSequenceType()),
                            result,
                            terminalSelector);

                    return new NavigationExpansionContextExpression(terminator, newContext);
                }
                else if (expandedAny)
                {
                    return CreateCall(
                        node.Method
                            .GetGenericMethodDefinition()
                            .MakeGenericMethod(new[]
                            {
                                source.Type.GetSequenceType(),
                                keySelector.ReturnType,
                                elementSelector.ReturnType,
                                resultSelector.ReturnType,
                            }),
                        new[]
                        {
                            source,
                            keySelector,
                            elementSelector,
                            resultSelector,
                        }.Concat(node.Arguments.Skip(4)));
                }
                else
                {
                    return node.Update(null, new[]
                    {
                        source,
                        keySelector,
                        elementSelector,
                        resultSelector,
                    }.Concat(node.Arguments.Skip(4)));
                }
            }

            private Expression HandleGroupByWithElementSelector(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var keySelector = Visit(node.Arguments[1]).UnwrapLambda();
                var elementSelector = Visit(node.Arguments[2]).UnwrapLambda();
                var keyParameter = keySelector.Parameters[0];
                var elementParameter = elementSelector.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(keyParameter, navigationDescriptors);
                }

                bool foundKeyNavigations, foundElementNavigations, expandedAny = false;

                var originalKeySelector = keySelector;
                var originalKeyParameter = keyParameter;
                var originalElementSelector = elementSelector;
                var originalElementParameter = elementParameter;

                do
                {
                    keySelector = originalKeySelector;
                    keyParameter = originalKeyParameter;
                    elementSelector = originalElementSelector;
                    elementParameter = originalElementParameter;

                    expandedAny
                        |= context.ExpandIntermediateLambda(
                            ref source,
                            ref keySelector,
                            ref keyParameter,
                            out foundKeyNavigations);

                    expandedAny
                        |= context.ExpandIntermediateLambda(
                            ref source,
                            ref elementSelector,
                            ref elementParameter,
                            out foundElementNavigations);
                }
                while (foundKeyNavigations || foundElementNavigations);

                if (expandedAny)
                {
                    return Expression.Call(
                        node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                            source.Type.GetSequenceType(),
                            keySelector.ReturnType,
                            elementSelector.ReturnType),
                        new[]
                        {
                            source,
                            keySelector,
                            elementSelector,
                        }.Concat(node.Arguments.Skip(3)));
                }
                else
                {
                    return node.Update(null, new[]
                    {
                        source,
                        keySelector,
                        elementSelector,
                    }.Concat(node.Arguments.Skip(3)));
                }
            }

            private Expression HandleGroupByWithResultSelector(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var keySelector = Visit(node.Arguments[1]).UnwrapLambda();
                var resultSelector = Visit(node.Arguments[2]).UnwrapLambda();
                var keyParameter = keySelector.Parameters[0];
                var resultKeyParameter = resultSelector.Parameters[0];
                var resultElementsParameter = resultSelector.Parameters[1];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(keyParameter, navigationDescriptors);
                }

                var expandedKeySelector
                    = context.ExpandIntermediateLambda(
                        ref source,
                        ref keySelector,
                        ref keyParameter,
                        out _);

                var intermediateResultType
                    = typeof(NavigationTransparentIdentifier<,>).MakeGenericType(
                        resultKeyParameter.Type,
                        resultElementsParameter.Type);

                var resultOuterField = intermediateResultType.GetRuntimeField("Outer");
                var resultInnerField = intermediateResultType.GetRuntimeField("Inner");

                var intermediateResultSelector
                    = Expression.Lambda(
                        Expression.New(
                            intermediateResultType.GetTypeInfo().DeclaredConstructors.Single(),
                            new[] { resultKeyParameter, resultElementsParameter },
                            new[] { resultOuterField, resultInnerField }),
                        new[] { resultKeyParameter, resultElementsParameter });

                var newGenericMethodDefinition
                    = node.Method.DeclaringType
                        .GetTypeInfo()
                        .GetDeclaredMethods("GroupBy")
                        .Select(m => new { m, p = m.GetParameters() })
                        .Where(x => x.p.Length == node.Arguments.Count + 1)
                        .Where(x => x.p.Any(p => p.Name == "elementSelector"))
                        .Where(x => x.p.Any(p => p.Name == "resultSelector"))
                        .Select(x => x.m)
                        .Single();

                var newSource
                    = CreateCall(
                        newGenericMethodDefinition
                            .MakeGenericMethod(new[]
                            {
                                    source.Type.GetSequenceType(),
                                    keySelector.ReturnType,
                                    node.Method.GetGenericArguments()[0],
                                    intermediateResultType,
                            }),
                        new[]
                        {
                                source,
                                keySelector,
                                context.OuterTerminalSelector,
                                intermediateResultSelector,
                        }.Concat(node.Arguments.Skip(4))) as Expression;

                var newResultParameter = Expression.Parameter(intermediateResultType, "<>nav");
                var newContext = new NavigationExpansionContext(newResultParameter, navigationDescriptors);

                var newResultSelector
                    = Expression.Lambda(
                        resultSelector.Body
                            .Replace(
                                resultKeyParameter,
                                Expression.MakeMemberAccess(newResultParameter, resultOuterField))
                            .Replace(
                                resultElementsParameter,
                                Expression.MakeMemberAccess(newResultParameter, resultInnerField)),
                        newResultParameter);

                if (newContext.ExpandResultLambda(ref newSource, ref newResultSelector, ref newResultParameter))
                {
                    var terminalSelectMethod = enumerableSelectMethodInfo;
                    var terminalSelector = (Expression)newContext.OuterTerminalSelector;

                    if (node.Method.IsQueryableMethod())
                    {
                        terminalSelectMethod = queryableSelectMethodInfo;
                        terminalSelector = Expression.Quote(terminalSelector);
                    }

                    var result
                        = Expression.Call(
                            terminalSelectMethod.MakeGenericMethod(
                                newSource.Type.GetSequenceType(),
                                newResultSelector.ReturnType),
                            newSource,
                            newResultSelector);

                    var terminator
                        = Expression.Call(
                            terminalSelectMethod.MakeGenericMethod(
                                newResultSelector.ReturnType,
                                node.Type.GetSequenceType()),
                            result,
                            terminalSelector);

                    return new NavigationExpansionContextExpression(terminator, newContext);
                }
                else if (expandedKeySelector)
                {
                    return CreateCall(
                        newGenericMethodDefinition
                            .MakeGenericMethod(new[]
                            {
                                keyParameter.Type,
                                keySelector.ReturnType,
                                node.Method.GetGenericArguments()[0],
                                node.Method.GetGenericArguments()[2],
                            }),
                        new[]
                        {
                            source,
                            keySelector,
                            context.OuterTerminalSelector,
                            resultSelector,
                        }.Concat(node.Arguments.Skip(3)));
                }
                else
                {
                    return node.Update(null, new[]
                    {
                        source,
                        keySelector,
                        resultSelector,
                    }.Concat(node.Arguments.Skip(3)));
                }
            }

            private Expression HandleGroupByWithoutElementAndResultSelectors(MethodCallExpression node)
            {
                var source = Visit(node.Arguments[0]);
                var keySelector = Visit(node.Arguments[1]).UnwrapLambda();
                var keyParameter = keySelector.Parameters[0];
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    context = new NavigationExpansionContext(keyParameter, navigationDescriptors);
                }

                if (context.ExpandIntermediateLambda(ref source, ref keySelector, ref keyParameter, out _))
                {
                    return CreateCall(
                        node.Method.DeclaringType.GetTypeInfo()
                            .GetDeclaredMethods("GroupBy")
                            .Select(m => new { m, p = m.GetParameters() })
                            .Where(x => x.p.Length == node.Arguments.Count + 1)
                            .Where(x => x.p.Any(p => p.Name == "elementSelector"))
                            .Select(x => x.m)
                            .Single()
                            .MakeGenericMethod(new[]
                            {
                                keyParameter.Type,
                                keySelector.ReturnType,
                                node.Method.GetGenericArguments()[0],
                            }),
                        new[]
                        {
                            source,
                            keySelector,
                            context.OuterTerminalSelector,
                        }.Concat(node.Arguments.Skip(2)));
                }
                else
                {
                    return node.Update(null, new[]
                    {
                        source,
                        keySelector,
                    }.Concat(node.Arguments.Skip(2)));
                }
            }

            private Expression HandleGroupJoin(MethodCallExpression node)
            {
                var outerSource = Visit(node.Arguments[0]);
                var innerSource = Visit(node.Arguments[1]);
                var outerKeySelector = Visit(node.Arguments[2]).UnwrapLambda();
                var innerKeySelector = Visit(node.Arguments[3]).UnwrapLambda();
                var resultSelector = Visit(node.Arguments[4]).UnwrapLambda();
                var outerParameter = outerKeySelector.Parameters.Single();
                var innerParameter = innerKeySelector.Parameters.Single();
                var outerContext = default(NavigationExpansionContext);
                var innerContext = default(NavigationExpansionContext);

                if (outerSource is NavigationExpansionContextExpression outerNece)
                {
                    outerSource = outerNece.Source;
                    outerContext = outerNece.Context;
                }
                else
                {
                    outerContext = new NavigationExpansionContext(outerParameter, navigationDescriptors);
                }

                if (innerSource is NavigationExpansionContextExpression innerNece)
                {
                    innerSource = innerNece.Source;
                    innerContext = innerNece.Context;
                }
                else
                {
                    innerContext = new NavigationExpansionContext(innerParameter, navigationDescriptors);
                }

                var outerExpanded
                    = outerContext.ExpandIntermediateLambda(
                        ref outerSource,
                        ref outerKeySelector,
                        ref outerParameter,
                        out _);

                var innerExpanded
                    = innerContext.ExpandIntermediateLambda(
                        ref innerSource,
                        ref innerKeySelector,
                        ref innerParameter,
                        out _);

                if (outerExpanded || innerExpanded)
                {
                    // TODO: GroupJoin: navigations inside result selector

                    if (innerExpanded)
                    {
                        var oldResultInnerParameter = resultSelector.Parameters[1];

                        var newResultInnerParameter
                            = Expression.Parameter(
                                typeof(IEnumerable<>).MakeGenericType(innerParameter.Type),
                                oldResultInnerParameter.Name);

                        resultSelector
                             = Expression.Lambda(
                                 resultSelector.Body.Replace(
                                     oldResultInnerParameter,
                                     Expression.Call(
                                         enumerableSelectMethodInfo.MakeGenericMethod(
                                             newResultInnerParameter.Type.GetSequenceType(),
                                             oldResultInnerParameter.Type.GetSequenceType()),
                                         newResultInnerParameter,
                                         innerContext.OuterTerminalSelector)),
                                 resultSelector.Parameters[0],
                                 newResultInnerParameter);
                    }

                    outerParameter = resultSelector.Parameters[0];

                    outerContext.ExpandResultLambda(
                        ref outerSource,
                        ref resultSelector,
                        ref outerParameter);

                    var result
                        = CreateCall(
                            node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                outerSource.Type.GetSequenceType(),
                                innerSource.Type.GetSequenceType(),
                                outerKeySelector.ReturnType,
                                resultSelector.ReturnType),
                            new[]
                            {
                                outerSource,
                                innerSource,
                                outerKeySelector,
                                innerKeySelector,
                                resultSelector,
                            });

                    var newParameter = Expression.Parameter(resultSelector.ReturnType);

                    Expression outerTerminalSelector
                        = outerExpanded
                            ? outerContext.OuterTerminalSelector
                            : Expression.Lambda(
                                body: newParameter,
                                name: "GroupJoinPassthroughSelector",
                                parameters: new[] { newParameter });

                    var terminalSelectMethod = enumerableSelectMethodInfo;

                    if (node.Method.IsQueryableMethod())
                    {
                        outerTerminalSelector = Expression.Quote(outerTerminalSelector);
                        terminalSelectMethod = queryableSelectMethodInfo;
                    }

                    var terminator
                        = Expression.Call(
                            terminalSelectMethod.MakeGenericMethod(
                                resultSelector.ReturnType,
                                node.Type.GetSequenceType()),
                            result,
                            outerTerminalSelector);

                    return new NavigationExpansionContextExpression(terminator, outerContext);
                }
                else
                {
                    return node.Update(null, new[]
                    {
                        outerSource,
                        innerSource,
                        outerKeySelector,
                        innerKeySelector,
                        resultSelector,
                    }.Concat(node.Arguments.Skip(5)));
                }
            }

            private Expression HandleJoin(MethodCallExpression node)
            {
                var outerSource = Visit(node.Arguments[0]);
                var innerSource = Visit(node.Arguments[1]);
                var outerKeySelector = Visit(node.Arguments[2]).UnwrapLambda();
                var innerKeySelector = Visit(node.Arguments[3]).UnwrapLambda();
                var resultSelector = Visit(node.Arguments[4]).UnwrapLambda();
                var outerParameter = outerKeySelector.Parameters.Single();
                var innerParameter = innerKeySelector.Parameters.Single();
                var outerContext = default(NavigationExpansionContext);
                var innerContext = default(NavigationExpansionContext);

                if (outerSource is NavigationExpansionContextExpression outerNece)
                {
                    outerSource = outerNece.Source;
                    outerContext = outerNece.Context;
                }
                else
                {
                    outerContext = new NavigationExpansionContext(outerParameter, navigationDescriptors);
                }

                if (innerSource is NavigationExpansionContextExpression innerNece)
                {
                    innerSource = innerNece.Source;
                    innerContext = innerNece.Context;
                }
                else
                {
                    innerContext = new NavigationExpansionContext(innerParameter, navigationDescriptors);
                }

                var outerExpanded
                    = outerContext.ExpandIntermediateLambda(
                        ref outerSource,
                        ref outerKeySelector,
                        ref outerParameter,
                        out _);

                var innerExpanded
                    = innerContext.ExpandIntermediateLambda(
                        ref innerSource,
                        ref innerKeySelector,
                        ref innerParameter,
                        out _);

                if (outerExpanded || innerExpanded)
                {
                    // TODO: Join: navigations inside result selector

                    var resultContext
                        = NavigationExpansionContext.Merge(
                            outerContext,
                            outerParameter,
                            innerContext,
                            innerParameter,
                            ref resultSelector);

                    var result
                        = CreateCall(
                            node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                outerSource.Type.GetSequenceType(),
                                innerSource.Type.GetSequenceType(),
                                outerKeySelector.ReturnType,
                                resultSelector.ReturnType),
                            new[]
                            {
                                outerSource,
                                innerSource,
                                outerKeySelector,
                                innerKeySelector,
                                resultSelector,
                            });

                    return new NavigationExpansionContextExpression(
                        CreateTerminalCall(node, result, resultContext),
                        resultContext);
                }
                else
                {
                    return node.Update(null, new[]
                    {
                        outerSource,
                        innerSource,
                        outerKeySelector,
                        innerKeySelector,
                        resultSelector,
                    }.Concat(node.Arguments.Skip(5)));
                }
            }

            private Expression HandleZip(MethodCallExpression node)
            {
                var outerSource = Visit(node.Arguments[0]);
                var innerSource = Visit(node.Arguments[1]);
                var resultSelector = Visit(node.Arguments[2]).UnwrapLambda();
                var outerParameter = resultSelector.Parameters[0];
                var innerParameter = resultSelector.Parameters[1];
                var outerContext = default(NavigationExpansionContext);
                var innerContext = default(NavigationExpansionContext);

                if (outerSource is NavigationExpansionContextExpression outerNece)
                {
                    outerSource = outerNece.Source;
                    outerContext = outerNece.Context;
                }
                else
                {
                    outerContext = new NavigationExpansionContext(outerParameter, navigationDescriptors);
                }

                if (innerSource is NavigationExpansionContextExpression innerNece)
                {
                    innerSource = innerNece.Source;
                    innerContext = innerNece.Context;
                }
                else
                {
                    innerContext = new NavigationExpansionContext(innerParameter, navigationDescriptors);
                }

                var outerExpanded
                    = outerContext.ExpandIntermediateLambda(
                        ref outerSource,
                        ref resultSelector,
                        ref outerParameter,
                        out _);

                var innerExpanded
                    = innerContext.ExpandIntermediateLambda(
                        ref innerSource,
                        ref resultSelector,
                        ref innerParameter,
                        out _);

                if (outerExpanded || innerExpanded)
                {
                    // TODO: Zip: navigations inside result selector

                    var resultContext
                        = NavigationExpansionContext.Merge(
                            outerContext,
                            outerParameter,
                            innerContext,
                            innerParameter,
                            ref resultSelector,
                            applyExpansions: false);

                    var result
                        = CreateCall(
                            node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                outerSource.Type.GetSequenceType(),
                                innerSource.Type.GetSequenceType(),
                                resultSelector.ReturnType),
                            new[]
                            {
                                outerSource,
                                innerSource,
                                resultSelector,
                            });

                    return new NavigationExpansionContextExpression(
                        CreateTerminalCall(node, result, resultContext),
                        resultContext);
                }
                else
                {
                    return node.Update(null, new[]
                    {
                        outerSource,
                        innerSource,
                        resultSelector,
                    }.Concat(node.Arguments.Skip(3)));
                }
            }
        }

        /// <summary>
        /// This expression visitor finds all <see cref="NavigationExpansionContextExpression"/> instances
        /// and collapses them to their terminal expression.
        /// </summary>
        private sealed class NavigationContextTerminatingExpressionVisitor : ExpressionVisitor
        {
            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case NavigationExpansionContextExpression nece:
                    {
                        return Visit(nece.Result);
                    }

                    default:
                    {
                        return base.Visit(node);
                    }
                }
            }
        }

        /// <summary>
        /// This expression represents a subtree that is currently being expanded by the navigation
        /// rewriting expression visitor. 
        /// </summary>
        private sealed class NavigationExpansionContextExpression : Expression
        {
            public NavigationExpansionContextExpression(
                MethodCallExpression result,
                NavigationExpansionContext context)
            {
                Result = result ?? throw new ArgumentNullException(nameof(result));
                Context = context ?? throw new ArgumentNullException(nameof(context));
            }

            public Expression Source => Result.Arguments[0];

            public MethodCallExpression Result { get; }

            public NavigationExpansionContext Context { get; }

            public override ExpressionType NodeType => ExpressionType.Extension;

            public override Type Type => Result.Type;

            protected override Expression VisitChildren(ExpressionVisitor visitor)
            {
                var result = visitor.VisitAndConvert(Result, nameof(VisitChildren));

                if (result != Result)
                {
                    return new NavigationExpansionContextExpression(result, Context);
                }

                return this;
            }
        }

        /// <summary>
        /// This struct represents the anonymous 'transparent identifier' type
        /// to be used within the injected result selectors to Join, GroupJoin, etc.
        /// </summary>
        private struct NavigationTransparentIdentifier<TOuter, TInner>
        {
            public NavigationTransparentIdentifier(TOuter outer, TInner inner)
            {
                Outer = outer;
                Inner = inner;
            }

            [PathSegmentName("$outer")]
            public TOuter Outer;

            [PathSegmentName("$inner")]
            public TInner Inner;
        }

        private sealed class ExpansionMapping
        {
            public List<MemberInfo> OldPath;
            public List<MemberInfo> NewPath;
            public bool Nullable;
        }

        private struct FoundNavigation
        {
            public NavigationDescriptor Descriptor;
            public IEnumerable<MemberInfo> Path;
            public Type SourceType;
            public Type DestinationType;
        }

        private sealed class NavigationExpansionContext
        {
            private ParameterExpression currentParameter;
            private IEnumerable<NavigationDescriptor> descriptors;
            private List<ExpansionMapping> mappings = new List<ExpansionMapping>();
            private Stack<MemberInfo> terminalPath = new Stack<MemberInfo>();

            public NavigationExpansionContext(
                ParameterExpression parameter,
                IEnumerable<NavigationDescriptor> descriptors)
            {
                currentParameter = parameter ?? throw new ArgumentNullException(nameof(parameter));

                this.descriptors = descriptors ?? throw new ArgumentNullException(nameof(descriptors));

                mappings.Add(new ExpansionMapping
                {
                    OldPath = new List<MemberInfo>(),
                    NewPath = new List<MemberInfo>(),
                });
            }

            public LambdaExpression OuterTerminalSelector
                => Expression.Lambda(
                    terminalPath.Aggregate(currentParameter as Expression, Expression.MakeMemberAccess),
                    currentParameter);

            /// <summary>
            /// Expands navigations within an 'intermediate lambda' such as 
            /// the 'collectionSelector' parameter to SelectMany or the predicate
            /// to Where.
            /// </summary>
            /// <param name="source">The source query expression.</param>
            /// <param name="lambda">The <see cref="LambdaExpression"/>.</param>
            /// <param name="parameter">The parameter to the intermediate lambda, typically the first or only parameter.</param>
            /// <param name="foundNavigations">Whether or not any navigations were found in the <paramref name="lambda"/>.</param>
            /// <returns>Whether or not the <paramref name="lambda"/> was expanded.</returns>
            public bool ExpandIntermediateLambda(
                ref Expression source,
                ref LambdaExpression lambda,
                ref ParameterExpression parameter,
                out bool foundNavigations)
            {
                foundNavigations = ProcessNavigations(ref source, lambda, parameter);

                if (!foundNavigations && terminalPath.Count == 0)
                {
                    return false;
                }

                var body = ExpandLambdaBody(lambda.Body, parameter, currentParameter, mappings);

                if (lambda.ReturnType.IsCollectionType())
                {
                    body = body.AsCollectionType();
                }

                var parameters = SwapParameter(lambda.Parameters, parameter, currentParameter);

                var delegateType
                    = lambda.Type.GetGenericTypeDefinition().MakeGenericType(
                        parameters.Select(p => p.Type).Append(lambda.ReturnType).ToArray());

                lambda = Expression.Lambda(delegateType, body, parameters);

                parameter = currentParameter;

                return true;
            }

            /// <summary>
            /// Expands navigations within a 'result lambda' such as the 'resultSelector'
            /// parameters to Select, SelectMany, GroupJoin, or GroupBy.
            /// </summary>
            /// <param name="source">The source query expression.</param>
            /// <param name="lambda">The <see cref="LambdaExpression"/>.</param>
            /// <param name="outerParameter"></param>
            /// <returns>Whether or not the <paramref name="lambda"/> was expanded.</returns>
            public bool ExpandResultLambda(
                ref Expression source,
                ref LambdaExpression lambda,
                ref ParameterExpression parameter)
            {
                if (!ProcessNavigations(ref source, lambda, parameter) && terminalPath.Count == 0)
                {
                    return false;
                }

                var scopeType
                    = typeof(NavigationTransparentIdentifier<,>).MakeGenericType(
                        currentParameter.Type,
                        lambda.ReturnType);

                var outerField = scopeType.GetRuntimeField("Outer");
                var innerField = scopeType.GetRuntimeField("Inner");

                var newMappings = Remap(lambda, parameter, mappings);

                newMappings.ForEach(m => m.NewPath.Insert(0, outerField));

                if (lambda.Parameters.Count == 2)
                {
                    var innerParameterVisitor
                        = new SelectorInnerParameterMappingExpressionVisitor(lambda.Parameters[1], mappings);

                    innerParameterVisitor.Visit(lambda.Body);

                    innerParameterVisitor.NewMappings.ForEach(m => m.NewPath.Insert(0, innerField));

                    newMappings.AddRange(innerParameterVisitor.NewMappings);
                }

                var lambdaBody = ExpandLambdaBody(lambda.Body, parameter, currentParameter, mappings);

                if (innerField.FieldType.IsCollectionType())
                {
                    lambdaBody = lambdaBody.AsCollectionType();
                }

                lambda
                    = Expression.Lambda(
                        Expression.New(
                            scopeType.GetTypeInfo().DeclaredConstructors.Single(),
                            new[] { currentParameter, lambdaBody },
                            new[] { outerField, innerField }),
                        SwapParameter(lambda.Parameters, parameter, currentParameter));

                parameter = currentParameter;

                currentParameter = Expression.Parameter(scopeType, "<>nav");

                terminalPath.Clear();
                terminalPath.Push(innerField);

                mappings.Clear();

                mappings.Add(new ExpansionMapping
                {
                    OldPath = new List<MemberInfo>(),
                    NewPath = new List<MemberInfo> { innerField },
                });

                mappings.AddRange(newMappings);

                return true;
            }

            private bool ProcessNavigations(
                ref Expression source,
                LambdaExpression lambda,
                ParameterExpression parameter)
            {
                var findingVisitor = new NavigationFindingExpressionVisitor(parameter, descriptors, mappings);

                findingVisitor.Visit(lambda.Body);

                if (!findingVisitor.FoundNavigations.Any())
                {
                    return false;
                }

                var operatorType = source.Type.GetInterfaces().Contains(typeof(IQueryable)) ? typeof(Queryable) : typeof(Enumerable);

                foreach (var navigation in findingVisitor.FoundNavigations.OrderBy(f => f.Path.Count()))
                {
                    var outerType = source.Type.GetSequenceType();
                    var innerType = navigation.DestinationType.FindGenericType(typeof(IEnumerable<>)) ?? navigation.DestinationType;
                    var scopeType = typeof(NavigationTransparentIdentifier<,>).MakeGenericType(outerType, innerType);

                    var outerField = scopeType.GetRuntimeField("Outer");
                    var innerField = scopeType.GetRuntimeField("Inner");

                    var targetOldPath = navigation.Path.SkipLast(1).ToList();

                    var targetMapping = mappings.FirstOrDefault(m => m.OldPath.SequenceEqual(targetOldPath));

                    if (targetMapping == null)
                    {
                        targetMapping = new ExpansionMapping
                        {
                            OldPath = targetOldPath.ToList(),
                            NewPath = terminalPath.Concat(targetOldPath).ToList(),
                        };

                        mappings.Add(targetMapping);
                    }

                    var outerKeyPath
                        = targetMapping.NewPath.Concat(
                            navigation.Path.Except(targetMapping.OldPath).SkipLast(1));

                    var outerKeySelector
                        = Expression.Lambda(
                            navigation.Descriptor.OuterKeySelector.Body.Replace(
                                navigation.Descriptor.OuterKeySelector.Parameters.Single(),
                                outerKeyPath.Aggregate(currentParameter as Expression, Expression.MakeMemberAccess)),
                            currentParameter);

                    var innerKeySelector
                        = navigation.Descriptor.InnerKeySelector;

                    var nullableKeyType
                        = navigation.Descriptor.OuterKeySelector.ReturnType.IsNullableType()
                            || navigation.Descriptor.InnerKeySelector.ReturnType.IsNullableType();

                    var keyType
                        = nullableKeyType
                            ? outerKeySelector.ReturnType.MakeNullableType()
                            : outerKeySelector.ReturnType;

                    if (nullableKeyType)
                    {
                        outerKeySelector
                            = Expression.Lambda(
                                Expression.Convert(outerKeySelector.Body, keyType),
                                outerKeySelector.Parameters);

                        innerKeySelector
                            = Expression.Lambda(
                                Expression.Convert(innerKeySelector.Body, keyType),
                                innerKeySelector.Parameters);
                    }

                    var innerParameter
                        = Expression.Parameter(innerType, innerKeySelector.Parameters.Single().Name);

                    // We need to uniquify the tables in the expansion for scenarios like:
                    // from o1 in orders from o2 in orders where o1.Customer == o2.Customer select new { o1, o2 }
                    // Otherwise, both of the Customer expansions would be using the exact same table expression.
                    var expansion
                        = new TableUniquifyingExpressionVisitor().Visit(navigation.Descriptor.Expansion);

                    if (navigation.DestinationType.IsSequenceType())
                    {
                        var method
                            = operatorType
                                .GetRuntimeMethods()
                                .Single(m => m.Name == nameof(Queryable.GroupJoin) && m.GetParameters().Length == 5)
                                .MakeGenericMethod(currentParameter.Type, innerType.GetSequenceType(), keyType, scopeType);

                        // Create the result selector
                        var resultSelector
                            = Expression.Lambda(
                                Expression.New(
                                    scopeType.GetTypeInfo().DeclaredConstructors.Single(),
                                    new[] { currentParameter, innerParameter },
                                    new[] { outerField, innerField }),
                                new[] { currentParameter, innerParameter });

                        source
                            = Expression.Call(
                                method,
                                source,
                                expansion,
                                outerKeySelector,
                                innerKeySelector,
                                resultSelector);
                    }
                    else if (navigation.Descriptor.IsNullable || targetMapping.Nullable)
                    {
                        var innerEnumerableType = innerType.MakeEnumerableType();

                        var intermediateScopeType
                            = typeof(NavigationTransparentIdentifier<,>)
                                .MakeGenericType(outerType, innerEnumerableType);

                        var intermediateOuterField = intermediateScopeType.GetRuntimeField("Outer");
                        var intermediateInnerField = intermediateScopeType.GetRuntimeField("Inner");

                        var groupJoinMethod
                            = operatorType
                                .GetRuntimeMethods()
                                .Single(m => m.Name == nameof(Queryable.GroupJoin) && m.GetParameters().Length == 5)
                                .MakeGenericMethod(currentParameter.Type, innerType, keyType, intermediateScopeType);

                        // Expand mappings into the outer key selector

                        var innerEnumerableParameter
                            = Expression.Parameter(
                                innerEnumerableType,
                                innerKeySelector.Parameters.Single().Name + "s");

                        // Create the result selector
                        var resultSelector
                            = Expression.Lambda(
                                name: "NavigationExpandedResultSelector",
                                body: Expression.New(
                                    intermediateScopeType.GetTypeInfo().DeclaredConstructors.Single(),
                                    new[] { currentParameter, innerEnumerableParameter },
                                    new[] { intermediateOuterField, intermediateInnerField }),
                                parameters: new[] { currentParameter, innerEnumerableParameter });

                        var groupJoinCall
                            = Expression.Call(
                                groupJoinMethod,
                                source,
                                expansion,
                                outerKeySelector,
                                innerKeySelector,
                                resultSelector);

                        var intermediateScopeParameter = Expression.Parameter(intermediateScopeType);

                        var selectManyCollectionSelector
                            = Expression.Lambda(
                                name: "OneToOneOptionalSelectManyCollectionSelector",
                                body: Expression.Call(
                                    typeof(Enumerable).GetTypeInfo().DeclaredMethods
                                        .Single(m => m.Name == nameof(Enumerable.DefaultIfEmpty)
                                            && m.GetParameters().Length == 1).MakeGenericMethod(innerType),
                                    Expression.MakeMemberAccess(intermediateScopeParameter, intermediateInnerField)),
                                parameters: new[] { intermediateScopeParameter });

                        var selectManyResultSelector
                            = Expression.Lambda(
                                name: "OneToOneOptionalSelectManyResultSelector",
                                body: Expression.New(
                                    scopeType.GetTypeInfo().DeclaredConstructors.Single(),
                                    new Expression[]
                                    {
                                        Expression.MakeMemberAccess(
                                            intermediateScopeParameter,
                                            intermediateScopeType.GetRuntimeField("Outer")),
                                        innerParameter
                                    },
                                    new[] { outerField, innerField }),
                                parameters: new[] { intermediateScopeParameter, innerParameter });

                        var selectManyMethod
                             = (from m in operatorType.GetRuntimeMethods()
                                where m.Name == nameof(Queryable.SelectMany)
                                let p = m.GetParameters()
                                where p.Length == 3
                                let t = p[1].ParameterType.GetGenericTypeDefinition() == typeof(Expression<>)
                                    ? p[1].ParameterType.GetGenericArguments()[0]
                                    : p[1].ParameterType
                                where t.GenericTypeArguments.Length == 2
                                select m).Single().MakeGenericMethod(intermediateScopeType, innerType, scopeType);

                        source
                            = Expression.Call(
                                selectManyMethod,
                                groupJoinCall,
                                selectManyCollectionSelector,
                                selectManyResultSelector);
                    }
                    else
                    {
                        var method
                            = operatorType
                                .GetRuntimeMethods()
                                .Single(m => m.Name == nameof(Queryable.Join) && m.GetParameters().Length == 5)
                                .MakeGenericMethod(currentParameter.Type, innerType, keyType, scopeType);

                        // Create the result selector
                        var resultSelector
                            = Expression.Lambda(
                                Expression.New(
                                    scopeType.GetTypeInfo().DeclaredConstructors.Single(),
                                    new[] { currentParameter, innerParameter },
                                    new[] { outerField, innerField }),
                                new[] { currentParameter, innerParameter });

                        source
                            = Expression.Call(
                                method,
                                source,
                                expansion,
                                outerKeySelector,
                                innerKeySelector,
                                resultSelector);
                    }

                    currentParameter = Expression.Parameter(scopeType, "<>nav");

                    mappings.ForEach(m => m.NewPath.Insert(0, outerField));

                    mappings.Add(new ExpansionMapping
                    {
                        OldPath = navigation.Path.ToList(),
                        NewPath = new List<MemberInfo> { innerField },
                        Nullable = navigation.Descriptor.IsNullable || targetMapping.Nullable,
                    });

                    terminalPath.Push(outerField);
                }

                return true;
            }

            private static Expression ExpandLambdaBody(
                Expression lambdaBody,
                ParameterExpression oldParameter,
                ParameterExpression newParameter,
                List<ExpansionMapping> mappings)
            {
                var visitor = new NavigationExpandingExpressionVisitor(oldParameter, newParameter, mappings);

                return visitor.Visit(lambdaBody);
            }

            private static IEnumerable<ParameterExpression> SwapParameter(
                IEnumerable<ParameterExpression> parameters,
                ParameterExpression oldParameter,
                ParameterExpression newParameter)
            {
                var newParameters = parameters.ToArray();

                for (var i = 0; i < newParameters.Length; i++)
                {
                    if (newParameters[i] == oldParameter)
                    {
                        newParameters[i] = newParameter;
                        break;
                    }
                }

                return newParameters;
            }

            private static List<ExpansionMapping> Remap(
                LambdaExpression lambda,
                ParameterExpression parameter,
                List<ExpansionMapping> mappings)
            {
                var mappingVisitor = new SelectorMappingExpressionVisitor(parameter, mappings);

                mappingVisitor.Visit(lambda.Body);

                return mappingVisitor.NewMappings;
            }

            public static NavigationExpansionContext Merge(
                NavigationExpansionContext outerContext,
                ParameterExpression outerParameter,
                NavigationExpansionContext innerContext,
                ParameterExpression innerParameter,
                ref LambdaExpression lambda,
                bool applyExpansions = true)
            {
                var outerMappings = Remap(lambda, outerParameter, outerContext.mappings);
                var innerMappings = Remap(lambda, innerParameter, innerContext.mappings);

                // Merge scope

                var mergeScopeType
                    = typeof(NavigationTransparentIdentifier<,>).MakeGenericType(
                        outerContext.currentParameter.Type,
                        innerContext.currentParameter.Type);

                var mergeOuterField = mergeScopeType.GetRuntimeField("Outer");
                var mergeInnerField = mergeScopeType.GetRuntimeField("Inner");

                outerMappings.ForEach(m => m.NewPath.Insert(0, mergeOuterField));
                innerMappings.ForEach(m => m.NewPath.Insert(0, mergeInnerField));

                // Result scope

                var resultScopeType
                    = typeof(NavigationTransparentIdentifier<,>).MakeGenericType(
                        mergeScopeType,
                        lambda.ReturnType);

                var resultOuterField = resultScopeType.GetRuntimeField("Outer");
                var resultInnerField = resultScopeType.GetRuntimeField("Inner");

                var resultMappings = outerMappings.Concat(innerMappings).ToList();

                resultMappings.ForEach(m => m.NewPath.Insert(0, resultOuterField));

                resultMappings.Insert(0, new ExpansionMapping
                {
                    OldPath = new List<MemberInfo>(),
                    NewPath = new List<MemberInfo> { resultInnerField },
                });

                // Lambda

                var lambdaBody = lambda.Body;

                if (applyExpansions)
                {
                    lambdaBody = ExpandLambdaBody(lambdaBody, lambda.Parameters[0], outerParameter, outerContext.mappings);
                    lambdaBody = ExpandLambdaBody(lambdaBody, lambda.Parameters[1], innerParameter, innerContext.mappings);
                }

                lambda
                    = Expression.Lambda(
                        Expression.New(
                            mergeScopeType.GetTypeInfo().DeclaredConstructors.Single(),
                            new[] { outerParameter, innerParameter },
                            new[] { mergeOuterField, mergeInnerField }),
                        outerParameter,
                        innerParameter);

                lambda
                    = Expression.Lambda(
                        Expression.New(
                            resultScopeType.GetTypeInfo().DeclaredConstructors.Single(),
                            new[] { lambda.Body, lambdaBody },
                            new[] { resultOuterField, resultInnerField }),
                        outerParameter,
                        innerParameter);

                var resultContext
                    = new NavigationExpansionContext(
                        Expression.Parameter(resultScopeType, "<>nav"),
                        outerContext.descriptors.Union(innerContext.descriptors));

                resultContext.mappings.Clear();
                resultContext.mappings.AddRange(resultMappings);
                resultContext.terminalPath.Push(resultInnerField);

                return resultContext;
            }
        }

        private static void UnwindMemberExpression(
            MemberExpression memberExpression,
            out Expression innerExpression,
            out List<MemberInfo> path)
        {
            path = new List<MemberInfo>();

            do
            {
                path.Add(memberExpression.Member);

                innerExpression = memberExpression.Expression.UnwrapInnerExpression();

                memberExpression = innerExpression as MemberExpression;
            }
            while (memberExpression != null);

            path.Reverse();
        }

        private sealed class NavigationFindingExpressionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression targetParameter;
            private readonly IEnumerable<NavigationDescriptor> descriptors;
            private readonly IEnumerable<ExpansionMapping> mappings;

            public List<FoundNavigation> FoundNavigations { get; } = new List<FoundNavigation>();

            public NavigationFindingExpressionVisitor(
                ParameterExpression targetParameter,
                IEnumerable<NavigationDescriptor> descriptors,
                IEnumerable<ExpansionMapping> mappings)
            {
                this.targetParameter = targetParameter;
                this.descriptors = descriptors;
                this.mappings = mappings;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var descriptor = descriptors.SingleOrDefault(d => d.Member == node.Member);

                if (descriptor == null)
                {
                    return base.VisitMember(node);
                }

                UnwindMemberExpression(node, out var expression, out var path);

                if (expression == targetParameter)
                {
                    if (mappings.Any(m => m.OldPath.SequenceEqual(path))
                        || FoundNavigations.Any(f => f.Path.SequenceEqual(path)))
                    {
                        return node;
                    }
                    else
                    {
                        FoundNavigations.Add(new FoundNavigation
                        {
                            Descriptor = descriptor,
                            Path = path,
                            SourceType = node.Expression.Type,
                            DestinationType = node.Type,
                        });
                    }
                }

                return base.VisitMember(node);
            }
        }

        private sealed class NavigationExpandingExpressionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression oldParameter;
            private readonly ParameterExpression newParameter;
            private readonly IEnumerable<ExpansionMapping> mappings;

            public NavigationExpandingExpressionVisitor(
                ParameterExpression oldParameter,
                ParameterExpression newParameter,
                IEnumerable<ExpansionMapping> mappings)
            {
                this.oldParameter = oldParameter;
                this.newParameter = newParameter;
                this.mappings = mappings;
            }

            protected override Expression VisitNew(NewExpression node)
            {
                return node.Update(node.Arguments.Select(MaybeToList));
            }

            private Expression MaybeToList(Expression original)
            {
                if (original == null)
                {
                    return original;
                }

                var visited = Visit(original);

                if (original.Type.IsCollectionType())
                {
                    visited = visited.AsCollectionType();
                }

                return visited;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var @object = MaybeToList(node.Object);
                var arguments = node.Arguments.Select(MaybeToList);

                return node.Update(@object, arguments);
            }

            protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
            {
                var expression = MaybeToList(node.Expression);

                return node.Update(expression);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node == oldParameter)
                {
                    foreach (var mapping in mappings)
                    {
                        if (!mapping.OldPath.Any())
                        {
                            return mapping.NewPath.Aggregate(
                                newParameter as Expression,
                                Expression.MakeMemberAccess);
                        }
                    }
                }

                return base.VisitParameter(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                UnwindMemberExpression(node, out var expression, out var path);

                if (expression == oldParameter)
                {
                    foreach (var mapping in mappings)
                    {
                        if (mapping.OldPath.SequenceEqual(path))
                        {
                            return mapping.NewPath.Aggregate(
                                newParameter as Expression,
                                Expression.MakeMemberAccess);
                        }
                    }
                }

                if (node.Member.DeclaringType.IsCollectionType()
                    && node.Member.Name == nameof(ICollection<object>.Count))
                {
                    var visited = Visit(node.Expression);

                    if (!visited.Type.IsCollectionType())
                    {
                        return Expression.Call(
                            enumerableCountMethodInfo.MakeGenericMethod(node.Expression.Type.GetSequenceType()),
                            Visit(node.Expression));
                    }

                    return node.Update(visited);
                }

                return base.VisitMember(node);
            }
        }

        private sealed class SelectorInnerParameterMappingExpressionVisitor : ProjectionExpressionVisitor
        {
            private readonly ParameterExpression innerParameter;
            private readonly IEnumerable<ExpansionMapping> mappings;

            public List<ExpansionMapping> NewMappings { get; } = new List<ExpansionMapping>();

            public SelectorInnerParameterMappingExpressionVisitor(
                ParameterExpression innerParameter,
                IEnumerable<ExpansionMapping> mappings)
            {
                this.innerParameter = innerParameter;
                this.mappings = mappings;
            }

            protected override Expression VisitLeaf(Expression node)
            {
                if (node == innerParameter)
                {
                    NewMappings.Add(new ExpansionMapping
                    {
                        OldPath = CurrentPath.ToList(),
                        NewPath = CurrentPath.ToList(),
                    });
                }

                return node;
            }
        }

        private sealed class SelectorMappingExpressionVisitor : ProjectionExpressionVisitor
        {
            private readonly ParameterExpression oldParameter;
            private readonly IEnumerable<ExpansionMapping> mappings;

            public List<ExpansionMapping> NewMappings = new List<ExpansionMapping>();

            public SelectorMappingExpressionVisitor(
                ParameterExpression oldParameter,
                IEnumerable<ExpansionMapping> mappings)
            {
                this.oldParameter = oldParameter;
                this.mappings = mappings;
            }

            protected override Expression VisitLeaf(Expression node)
            {
                switch (node)
                {
                    case MemberExpression memberExpression:
                    {
                        UnwindMemberExpression(memberExpression, out var expression, out var inputPath);

                        if (expression == oldParameter)
                        {
                            var outputPath = CurrentPath.ToList();

                            foreach (var oldMapping in mappings)
                            {
                                if (oldMapping.OldPath.Take(inputPath.Count).SequenceEqual(inputPath))
                                {
                                    var oldPath = outputPath.Concat(oldMapping.OldPath.Skip(inputPath.Count)).ToList();

                                    Debug.Assert(IsValidMemberPath(oldPath));

                                    if (!NewMappings.Any(m => m.OldPath.SequenceEqual(oldPath)))
                                    {
                                        NewMappings.Add(new ExpansionMapping
                                        {
                                            OldPath = oldPath,
                                            NewPath = oldMapping.NewPath.ToList(),
                                        });
                                    }
                                }
                            }
                        }

                        return node;
                    }

                    default:
                    {
                        return node;
                    }
                }
            }
        }

        private static bool IsValidMemberPath(List<MemberInfo> members)
        {
            var type = members.ElementAtOrDefault(0)?.DeclaringType;

            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];

                if (member.DeclaringType.IsAssignableFrom(type))
                {
                    type = member.GetMemberType();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
