using Impatient.Extensions;
using Impatient.Metadata;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
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

            public override Expression Visit(Expression node)
            {
                var visited = base.Visit(node);

                if (!(node is null || visited is null)
                    && node.Type.IsGenericType(typeof(IOrderedQueryable<>))
                    && !visited.Type.IsGenericType(typeof(IOrderedQueryable<>)))
                {
                    visited = visited.AsOrderedQueryable();
                }

                return visited;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(ImpatientExtensions)
                    && node.Method.Name == nameof(ImpatientExtensions.AsOrderedQueryable))
                {
                    return HandlePassthroughMethod(node);
                }

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

                    // These will need more consideration
                    case nameof(Queryable.Cast):
                    case nameof(Queryable.OfType):
                        {
                            break;
                        }

                    // Pass-through methods (just need a generic type change)
                    case nameof(Enumerable.AsEnumerable):
                    case nameof(Queryable.AsQueryable):
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
                            return HandleOrderByMethod(node);
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

            private MethodCallExpression CreateCall(MethodInfo method, IEnumerable<Expression> arguments)
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

            private Expression HandleOrderByMethod(MethodCallExpression node)
            {
                // It's a list named 'stack'. So what?
                var stack = new List<(MethodCallExpression node, Expression selector)>();

                while (node?.Method.Name == nameof(Queryable.ThenBy)
                    || node?.Method.Name == nameof(Queryable.ThenByDescending))
                {
                    stack.Insert(0, (node, Visit(node.Arguments[1])));

                    node = node.Arguments[0] as MethodCallExpression;
                }

                if (node?.Method.Name != nameof(Queryable.OrderBy) &&
                    node?.Method.Name != nameof(Queryable.OrderByDescending))
                {
                    // TODO: Something better
                    throw new InvalidOperationException();
                }

                var source = Visit(node.Arguments[0]);
                var context = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    context = nece.Context;
                }
                else
                {
                    var parameter = node.Arguments[1].UnwrapLambda().Parameters[0];

                    context = new NavigationExpansionContext(parameter, navigationDescriptors);
                }

                stack.Insert(0, (node, Visit(node.Arguments[1])));

                var consumed = false;

                for (var i = 0; i < stack.Count; i++)
                {
                    var selector = stack[i].selector.UnwrapLambda();
                    var parameter = selector.Parameters[0];

                    consumed |= context.ConsumeNavigations(ref source, selector, parameter);
                }

                if (consumed || context.HasExpansions)
                {
                    var result = source;

                    for (var i = 0; i < stack.Count; i++)
                    {
                        var frame = stack[i];
                        var selector = frame.selector.UnwrapLambda();
                        var parameter = selector.Parameters[0];

                        context.ApplyMappings(ref selector, ref parameter);

                        result
                            = CreateCall(
                                frame.node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                    parameter.Type,
                                    frame.node.Method.GetGenericArguments()[1]),
                                new[] { result, selector });
                    }

                    return new NavigationExpansionContextExpression(
                        CreateTerminalCall(node, result, context),
                        context);
                }
                else
                {
                    var result = source;

                    foreach (var call in stack)
                    {
                        result = call.node.Update(null, new[] { result, call.selector }.Concat(call.node.Arguments.Skip(2)));
                    }

                    return result;
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

                var consumed = context.ConsumeNavigations(ref source, selector, parameter);

                if (consumed || context.HasExpansions)
                {
                    context.ApplyMappings(ref selector, ref parameter);

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

                var consumed = context.ConsumeNavigations(ref source, predicate, parameter);

                if (consumed || context.HasExpansions)
                {
                    context.ApplyMappings(ref predicate, ref parameter);

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

                var consumed = context.ConsumeNavigations(ref source, predicate, parameter);

                if (consumed || context.HasExpansions)
                {
                    context.ApplyMappings(ref predicate, ref parameter);

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

                var consumed = context.ConsumeNavigations(ref source, predicate, parameter);

                if (consumed || context.HasExpansions)
                {
                    context.ApplyMappings(ref predicate, ref parameter);

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

                var consumed = context.ConsumeNavigations(ref source, selector, parameter);

                if (consumed || context.HasExpansions)
                {
                    context = NavigationExpansionContext.Advance(context, ref selector, ref parameter);

                    var result
                        = CreateCall(
                            node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                source.Type.GetSequenceType(),
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

                var consumed = context.ConsumeNavigations(ref source, selector, parameter);

                if (consumed || context.HasExpansions)
                {
                    context.ApplyMappings(ref selector, ref parameter);

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
                var sourceParameter = collectionSelector.Parameters[0];
                var resultOuterParameter = resultSelector.Parameters[0];
                var resultInnerParameter = resultSelector.Parameters[1];
                var outerContext = default(NavigationExpansionContext);

                if (source is NavigationExpansionContextExpression nece)
                {
                    source = nece.Source;
                    outerContext = nece.Context;
                }
                else
                {
                    outerContext = new NavigationExpansionContext(sourceParameter, navigationDescriptors);
                }

                var consumed = false;

                consumed |= outerContext.ConsumeNavigations(ref source, collectionSelector, sourceParameter);
                consumed |= outerContext.ConsumeNavigations(ref source, resultSelector, resultOuterParameter);

                if (consumed || outerContext.HasExpansions)
                {
                    outerContext.ApplyMappings(ref collectionSelector, ref sourceParameter);
                    consumed |= true;
                }

                var innerSource = collectionSelector.Body;
                var innerContext = new NavigationExpansionContext(resultInnerParameter, navigationDescriptors);

                if (innerContext.ConsumeNavigations(ref innerSource, resultSelector, resultInnerParameter))
                {
                    collectionSelector = Expression.Lambda(innerSource, sourceParameter);
                    consumed |= true;
                }

                if (consumed)
                {
                    var resultContext
                        = NavigationExpansionContext.Advance(
                            outerContext,
                            resultOuterParameter,
                            innerContext,
                            resultInnerParameter,
                            ref resultSelector);

                    var result
                        = CreateCall(
                            node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                sourceParameter.Type,
                                collectionSelector.ReturnType.GetSequenceType(),
                                resultSelector.ReturnType),
                            new[] { source, collectionSelector, resultSelector });

                    return new NavigationExpansionContextExpression(
                        CreateTerminalCall(node, result, resultContext),
                        resultContext);
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

                var consumed = false;

                consumed |= context.ConsumeNavigations(ref source, keySelector, keyParameter);
                consumed |= context.ConsumeNavigations(ref source, elementSelector, elementParameter);

                if (consumed || context.HasExpansions)
                {
                    context.ApplyMappings(ref keySelector, ref keyParameter);
                    context.ApplyMappings(ref elementSelector, ref elementParameter);
                    consumed = true; // Because of context.HasExpansions
                }

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

                if (newContext.ConsumeNavigations(ref newSource, newResultSelector, newResultParameter))
                {
                    newContext
                        = NavigationExpansionContext.Advance(
                            newContext,
                            ref newResultSelector,
                            ref newResultParameter);

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
                else if (consumed)
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

                var consumed = false;

                consumed |= context.ConsumeNavigations(ref source, keySelector, keyParameter);
                consumed |= context.ConsumeNavigations(ref source, elementSelector, elementParameter);

                if (consumed || context.HasExpansions)
                {
                    context.ApplyMappings(ref keySelector, ref keyParameter);
                    context.ApplyMappings(ref elementSelector, ref elementParameter);

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

                var consumed = false;

                consumed |= context.ConsumeNavigations(ref source, keySelector, keyParameter);

                if (consumed || context.HasExpansions)
                {
                    context.ApplyMappings(ref keySelector, ref keyParameter);
                    consumed = true; // Because of context.HasExpansions
                }

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

                var groupByMethod
                    = node.Method.HasComparerArgument()
                        ? GetGenericMethodDefinition(
                            (IQueryable<object> q) => q.GroupBy(x => x, x => x, (x, y) => x, default))
                        : GetGenericMethodDefinition(
                            (IQueryable<object> q) => q.GroupBy(x => x, x => x, (x, y) => x));

                var newSource
                    = CreateCall(
                        groupByMethod
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

                if (newContext.ConsumeNavigations(ref newSource, newResultSelector, newResultParameter))
                {
                    newContext
                        = NavigationExpansionContext.Advance(
                            newContext,
                            ref newResultSelector,
                            ref newResultParameter);

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
                else if (consumed)
                {
                    return CreateCall(
                        groupByMethod
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

                var consumed = context.ConsumeNavigations(ref source, keySelector, keyParameter);

                if (consumed || context.HasExpansions)
                {
                    context.ApplyMappings(ref keySelector, ref keyParameter);

                    var groupByMethod
                        = node.Method.HasComparerArgument()
                            ? GetGenericMethodDefinition(
                                (IQueryable<object> q) => q.GroupBy(x => x, x => x, default))
                            : GetGenericMethodDefinition(
                                (IQueryable<object> q) => q.GroupBy(x => x, x => x));

                    return CreateCall(
                        groupByMethod
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
                var outerKeyParameter = outerKeySelector.Parameters.Single();
                var innerKeyParameter = innerKeySelector.Parameters.Single();
                var outerResultParameter = resultSelector.Parameters[0];
                var innerResultParameter = resultSelector.Parameters[1];
                var outerContext = default(NavigationExpansionContext);
                var innerContext = default(NavigationExpansionContext);

                if (outerSource is NavigationExpansionContextExpression outerNece)
                {
                    outerSource = outerNece.Source;
                    outerContext = outerNece.Context;
                }
                else
                {
                    outerContext = new NavigationExpansionContext(outerKeyParameter, navigationDescriptors);
                }

                if (innerSource is NavigationExpansionContextExpression innerNece)
                {
                    innerSource = innerNece.Source;
                    innerContext = innerNece.Context;
                }
                else
                {
                    innerContext = new NavigationExpansionContext(innerKeyParameter, navigationDescriptors);
                }

                var outerConsumed = false;
                var innerConsumed = false;

                outerConsumed |= outerContext.ConsumeNavigations(ref outerSource, outerKeySelector, outerKeyParameter);
                outerConsumed |= outerContext.ConsumeNavigations(ref outerSource, resultSelector, outerResultParameter);

                if (outerConsumed || outerContext.HasExpansions)
                {
                    outerContext.ApplyMappings(ref outerKeySelector, ref outerKeyParameter);

                    outerContext
                        = NavigationExpansionContext.Advance(
                            outerContext,
                            ref resultSelector,
                            ref outerResultParameter);

                    outerConsumed = true;
                }

                innerConsumed |= innerContext.ConsumeNavigations(ref innerSource, innerKeySelector, innerKeyParameter);

                if (innerConsumed || innerContext.HasExpansions)
                {
                    innerContext.ApplyMappings(ref innerKeySelector, ref innerKeyParameter);

                    innerConsumed = true;
                }

                if (outerConsumed || innerConsumed)
                {
                    if (innerConsumed)
                    {
                        var oldResultInnerParameter = resultSelector.Parameters[1];

                        var newResultInnerParameter
                            = Expression.Parameter(
                                typeof(IEnumerable<>).MakeGenericType(innerKeyParameter.Type),
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
                        = outerConsumed
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
                var outerKeyParameter = outerKeySelector.Parameters.Single();
                var innerKeyParameter = innerKeySelector.Parameters.Single();
                var outerResultParameter = resultSelector.Parameters[0];
                var innerResultParameter = resultSelector.Parameters[1];
                var outerContext = default(NavigationExpansionContext);
                var innerContext = default(NavigationExpansionContext);

                if (outerSource is NavigationExpansionContextExpression outerNece)
                {
                    outerSource = outerNece.Source;
                    outerContext = outerNece.Context;
                }
                else
                {
                    outerContext = new NavigationExpansionContext(outerKeyParameter, navigationDescriptors);
                }

                if (innerSource is NavigationExpansionContextExpression innerNece)
                {
                    innerSource = innerNece.Source;
                    innerContext = innerNece.Context;
                }
                else
                {
                    innerContext = new NavigationExpansionContext(innerKeyParameter, navigationDescriptors);
                }

                var consumed = false;

                consumed |= outerContext.ConsumeNavigations(ref outerSource, outerKeySelector, outerKeyParameter);
                consumed |= outerContext.ConsumeNavigations(ref outerSource, resultSelector, outerResultParameter);
                consumed |= outerContext.HasExpansions;

                consumed |= innerContext.ConsumeNavigations(ref innerSource, innerKeySelector, innerKeyParameter);
                consumed |= innerContext.ConsumeNavigations(ref innerSource, resultSelector, innerResultParameter);
                consumed |= innerContext.HasExpansions;

                if (consumed)
                {
                    outerContext.ApplyMappings(ref outerKeySelector, ref outerKeyParameter);
                    innerContext.ApplyMappings(ref innerKeySelector, ref innerKeyParameter);

                    var resultContext
                        = NavigationExpansionContext.Advance(
                            outerContext,
                            outerResultParameter,
                            innerContext,
                            innerResultParameter,
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

                var consumed = false;

                consumed |= outerContext.ConsumeNavigations(ref outerSource, resultSelector, outerParameter);
                consumed |= outerContext.HasExpansions;

                consumed |= innerContext.ConsumeNavigations(ref innerSource, resultSelector, innerParameter);
                consumed |= innerContext.HasExpansions;

                if (consumed)
                {
                    var resultContext
                        = NavigationExpansionContext.Advance(
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

        private sealed class ExpansionMapping : IEquatable<ExpansionMapping>
        {
            public List<MemberInfo> OldPath;
            public List<MemberInfo> NewPath;
            public ParameterExpression Parameter;
            public bool Nullable;

            public ExpansionMapping()
            {
            }

            public ExpansionMapping(
                ParameterExpression parameter,
                List<MemberInfo> oldPath,
                List<MemberInfo> newPath,
                bool nullable)
            {
                Parameter = parameter;
                OldPath = oldPath;
                NewPath = newPath;
                Nullable = nullable;
            }

            public override bool Equals(object obj)
            {
                return obj is ExpansionMapping other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = Parameter.GetHashCode();

                    for (var i = 0; i < OldPath.Count; i++)
                    {
                        hash = (hash * 16777619) ^ OldPath[i].GetHashCode();
                    }

                    for (var i = 0; i < NewPath.Count; i++)
                    {
                        hash = (hash * 16777619) ^ NewPath[i].GetHashCode();
                    }

                    hash = (hash * 16777619) ^ Nullable.GetHashCode();

                    return hash;
                }
            }

            public bool Equals(ExpansionMapping other)
            {
                return Equals(Parameter, other.Parameter)
                    && Equals(Nullable, other.Nullable)
                    && OldPath.SequenceEqual(other.OldPath)
                    && NewPath.SequenceEqual(other.NewPath);
            }

            public override string ToString()
            {
                var target = string.Join(".", NewPath.Select(m => m.Name).Prepend("$R"));
                var source = string.Join(".", OldPath.Select(m => m.Name).Prepend("$P"));

                return $"{source.PadRight(50)} => {target.PadRight(50)}";
            }
        }

        private struct FoundNavigation
        {
            public NavigationDescriptor Descriptor;
            public IEnumerable<MemberInfo> Path;
            public Type SourceType;
            public Type DestinationType;
            public bool Derived;
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
                    Parameter = currentParameter,
                });
            }

            public bool HasExpansions => terminalPath.Count != 0;

            public LambdaExpression OuterTerminalSelector
                => Expression.Lambda(
                    terminalPath.Aggregate(currentParameter as Expression, Expression.MakeMemberAccess),
                    currentParameter);

            /// <summary>
            /// Applies the context's current parameter and mappings to the given <paramref name="lambda"/>
            /// for all expressions stemming from the given <paramref name="parameter"/>, replacing by reference
            /// the <paramref name="lambda"/> and <paramref name="parameter"/> with their new forms.
            /// </summary>
            /// <param name="lambda">The <see cref="LambdaExpression"/>.</param>
            /// <param name="parameter">The <see cref="ParameterExpression"/>.</param>
            public void ApplyMappings(
                ref LambdaExpression lambda,
                ref ParameterExpression parameter)
            {
                var visitor = new NavigationExpandingExpressionVisitor(parameter, currentParameter, mappings);

                var lambdaBody = visitor.Visit(lambda.Body);

                if (lambda.ReturnType.IsCollectionType())
                {
                    lambdaBody = lambdaBody.AsCollectionType();
                }

                var parameters = SwapParameter(lambda.Parameters, parameter, currentParameter);

                var delegateType
                    = lambda.Type.GetGenericTypeDefinition().MakeGenericType(
                        parameters.Select(p => p.Type).Append(lambda.ReturnType).ToArray());

                lambda = Expression.Lambda(delegateType, lambdaBody, parameters);

                parameter = currentParameter;
            }

            /// <summary>
            /// Finds expandable navigations stemming from the given <paramref name="parameter"/>
            /// within the given <paramref name="lambda"/> then builds upon the given <paramref name="source"/>
            /// while advancing the context's 'current parameter' and recording mappings from the
            /// navigation paths to the new paths that can be used to access those values in the <paramref name="source"/>.
            /// </summary>
            /// <param name="source">The query expression tree up to this point.</param>
            /// <param name="lambda">
            ///     The <see cref="LambdaExpression"/> to process, like a key selector or result selector.
            /// </param>
            /// <param name="parameter">
            ///     The <see cref="ParameterExpression"/> to process navigations for. For key selectors this
            ///     will be the sole parameter of the <paramref name="lambda"/> lambda, but for result selectors
            ///     it could be either the 'outer'/'left' or 'inner'/'right' parameter.
            /// </param>
            /// <returns>Whether or not any navigations were found and consumed.</returns>
            public bool ConsumeNavigations(
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

                    var outerKeyPath = Enumerable.Empty<MemberInfo>();

                    if (targetMapping == null)
                    {
                        targetMapping = new ExpansionMapping
                        {
                            OldPath = targetOldPath.ToList(),
                            NewPath = terminalPath.Concat(targetOldPath).ToList(),
                            Parameter = parameter,
                        };

                        mappings.Add(targetMapping);

                        outerKeyPath =
                            targetMapping.NewPath.Concat(
                                navigation.Path
                                .Skip(targetMapping.OldPath.Count)
                                .SkipLast(1));
                    }
                    else
                    {
                        outerKeyPath = targetMapping.NewPath;
                    }

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

                    var expansion = navigation.Descriptor.Expansion;

                    // We need to uniquify the tables in the expansion for scenarios like:
                    // from o1 in orders from o2 in orders where o1.Customer == o2.Customer select new { o1, o2 }
                    // Otherwise, both of the Customer expansions would be using the exact same table expression.

                    expansion = new TableUniquifyingExpressionVisitor().Visit(expansion);

                    // We also want to visit the expansion itself in case it contains navigations.

                    expansion = new NavigationComposingExpressionVisitor(descriptors).Visit(expansion);

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
                    else if (navigation.Descriptor.IsNullable || navigation.Derived || targetMapping.Nullable)
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

                    mappings.ForEach(m =>
                    {
                        m.NewPath.Insert(0, outerField);
                        m.Parameter = currentParameter;
                    });

                    mappings.Add(new ExpansionMapping
                    {
                        OldPath = navigation.Path.ToList(),
                        NewPath = new List<MemberInfo> { innerField },
                        Nullable = navigation.Descriptor.IsNullable || navigation.Derived || targetMapping.Nullable,
                        Parameter = currentParameter,
                    });

                    terminalPath.Push(outerField);
                }

                return true;
            }

            public static NavigationExpansionContext Advance(
                NavigationExpansionContext context,
                ref LambdaExpression selector,
                ref ParameterExpression parameter)
            {
                // Result scope

                var resultScopeType
                    = typeof(NavigationTransparentIdentifier<,>).MakeGenericType(
                        context.currentParameter.Type,
                        selector.ReturnType);

                var resultOuterField = resultScopeType.GetRuntimeField("Outer");
                var resultInnerField = resultScopeType.GetRuntimeField("Inner");

                var resultMappings = GetResultMappings(context, selector, parameter);

                foreach (var mapping in resultMappings)
                {
                    mapping.NewPath.Insert(0, resultOuterField);
                    mapping.Parameter = context.currentParameter;
                };

                resultMappings.Insert(0, new ExpansionMapping
                {
                    OldPath = new List<MemberInfo> { },
                    NewPath = new List<MemberInfo> { resultInnerField },
                    Parameter = context.currentParameter,
                });

                var expander
                    = new NavigationExpandingExpressionVisitor(
                        parameter,
                        context.currentParameter,
                        context.mappings);

                var resultSelectorBody = expander.Visit(selector.Body);

                if (resultInnerField.FieldType.IsCollectionType())
                {
                    resultSelectorBody = resultSelectorBody.AsCollectionType();
                }

                selector
                    = Expression.Lambda(
                        Expression.New(
                            resultScopeType.GetTypeInfo().DeclaredConstructors.Single(),
                            new[] { context.currentParameter, resultSelectorBody },
                            new[] { resultOuterField, resultInnerField }),
                        // Use SwapParameter because there may be an extra argument, like index for Select
                        SwapParameter(selector.Parameters, parameter, context.currentParameter));

                parameter = context.currentParameter;

                // Result context

                var resultContext
                    = new NavigationExpansionContext(
                        Expression.Parameter(resultScopeType, "<>nav"),
                        context.descriptors);

                resultContext.mappings.Clear();
                resultContext.mappings.AddRange(resultMappings);
                resultContext.terminalPath.Push(resultInnerField);

                return resultContext;
            }

            public static NavigationExpansionContext Advance(
                NavigationExpansionContext outerContext,
                ParameterExpression outerParameter,
                NavigationExpansionContext innerContext,
                ParameterExpression innerParameter,
                ref LambdaExpression resultSelector)
            {
                // Merge scope

                var mergeScopeType
                    = typeof(NavigationTransparentIdentifier<,>).MakeGenericType(
                        outerContext.currentParameter.Type,
                        innerContext.currentParameter.Type);

                var mergeOuterField = mergeScopeType.GetRuntimeField("Outer");
                var mergeInnerField = mergeScopeType.GetRuntimeField("Inner");

                // Result scope

                var resultScopeType
                    = typeof(NavigationTransparentIdentifier<,>).MakeGenericType(
                        mergeScopeType,
                        resultSelector.ReturnType);

                var resultParameter
                    = Expression.Parameter(resultScopeType, "<>nav");

                var outerMappings = GetResultMappings(outerContext, resultSelector, outerParameter);
                var innerMappings = GetResultMappings(innerContext, resultSelector, innerParameter);

                outerMappings.ForEach(m =>
                {
                    m.NewPath.Insert(0, mergeOuterField);
                    m.Parameter = outerParameter;
                });

                innerMappings.ForEach(m =>
                {
                    m.NewPath.Insert(0, mergeInnerField);
                    m.Parameter = innerParameter;
                });

                var resultOuterField = resultScopeType.GetRuntimeField("Outer");
                var resultInnerField = resultScopeType.GetRuntimeField("Inner");

                var resultMappings = outerMappings.Concat(innerMappings).ToList();

                resultMappings.ForEach(m =>
                {
                    m.NewPath.Insert(0, resultOuterField);
                    m.Parameter = resultParameter;
                });

                resultMappings.Insert(0, new ExpansionMapping
                {
                    OldPath = new List<MemberInfo>(),
                    NewPath = new List<MemberInfo> { resultInnerField },
                    Parameter = resultParameter,
                });

                // Result selector

                var resultSelectorBody = resultSelector.Body;

                var outerExpander
                    = new NavigationExpandingExpressionVisitor(
                        outerParameter,
                        outerContext.currentParameter,
                        outerContext.mappings);

                resultSelectorBody = outerExpander.Visit(resultSelectorBody);

                var innerExpander
                    = new NavigationExpandingExpressionVisitor(
                        innerParameter,
                        innerContext.currentParameter,
                        innerContext.mappings);

                resultSelectorBody = innerExpander.Visit(resultSelectorBody);

                var resultSelectorOuterValue
                    = Expression.New(
                        mergeScopeType.GetTypeInfo().DeclaredConstructors.Single(),
                        new[] { outerContext.currentParameter, innerContext.currentParameter },
                        new[] { mergeOuterField, mergeInnerField });

                resultSelector
                    = Expression.Lambda(
                        Expression.New(
                            resultScopeType.GetTypeInfo().DeclaredConstructors.Single(),
                            new[] { resultSelectorOuterValue, resultSelectorBody },
                            new[] { resultOuterField, resultInnerField }),
                        outerContext.currentParameter,
                        innerContext.currentParameter);

                // Result context

                var resultContext
                    = new NavigationExpansionContext(
                        resultParameter,
                        outerContext.descriptors.Union(innerContext.descriptors));

                resultContext.mappings.Clear();
                resultContext.mappings.AddRange(resultMappings);
                resultContext.terminalPath.Push(resultInnerField);

                return resultContext;
            }

            private static List<ExpansionMapping> GetResultMappings(
                NavigationExpansionContext context,
                LambdaExpression selector,
                ParameterExpression parameter)
            {
                var gatherer = new SelectorGatheringExpressionVisitor(context.descriptors);

                gatherer.Visit(selector.Body);

                var directMappings = gatherer.Mappings.ToList();

                var indirectMappings = new List<ExpansionMapping>();

                foreach (var directMapping in directMappings)
                {
                    if (directMapping.Parameter != parameter)
                    {
                        continue;
                    }

                    var directMappingType
                        = directMapping.TargetPath
                            .Select(m => m.GetMemberType())
                            .Prepend(directMapping.Parameter.Type)
                            .Last();

                    foreach (var contextMapping in context.mappings)
                    {
                        var contextMappingType
                            = contextMapping.NewPath
                                .Select(m => m.GetMemberType())
                                .Prepend(contextMapping.Parameter.Type)
                                .Last();

                        //if (directMappingType.Equals(contextMappingType))
                        {
                            var head = contextMapping.OldPath.Take(directMapping.SourcePath.Count);
                            var tail = contextMapping.OldPath.Skip(directMapping.SourcePath.Count);

                            if (head.SequenceEqual(directMapping.SourcePath))
                            {
                                indirectMappings.Add(
                                    new ExpansionMapping(
                                        directMapping.Parameter,
                                        directMapping.TargetPath.Concat(tail).ToList(),
                                        contextMapping.NewPath.ToList(),
                                        contextMapping.Nullable));
                            }
                        }
                    }
                }

                return indirectMappings.Distinct(EqualityComparer<ExpansionMapping>.Default).ToList();
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
        }

        private static void UnwindMemberExpression(
            MemberExpression memberExpression,
            out Expression innerExpression,
            out List<MemberInfo> path)
        {
            path = new List<MemberInfo>();

            do
            {
                path.Insert(0, memberExpression.Member);

                innerExpression = memberExpression.Expression.UnwrapInnerExpression();

                memberExpression = innerExpression as MemberExpression;
            }
            while (memberExpression != null);
        }

        private static bool ContainsDerivedMembers(Expression expression, List<MemberInfo> path)
        {
            var currentType = expression.Type;

            for (var i = 0; i < path.Count; i++)
            {
                var member = path[i];

                if (!member.DeclaringType.IsAssignableFrom(currentType))
                {
                    return true;
                }

                currentType = member.GetMemberType();
            }

            return false;
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
                    if (mappings.Any(m => m.OldPath.SequenceEqual(path)))
                    {
                        return node;
                    }

                    if (FoundNavigations.Any(f => f.Path.SequenceEqual(path)))
                    {
                        return node;
                    }

                    FoundNavigations.Add(new FoundNavigation
                    {
                        Descriptor = descriptor,
                        Path = path,
                        SourceType = node.Expression.Type,
                        DestinationType = node.Type,
                        Derived = ContainsDerivedMembers(expression, path),
                    });
                }

                return base.VisitMember(node);
            }
        }

        private class NavigationExpandingExpressionVisitor : ExpressionVisitor
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

            protected override Expression VisitExtension(Expression node)
            {
                switch (node)
                {
                    case ExtendedNewExpression extendedNewExpression:
                        {
                            return VisitExtendedNew(extendedNewExpression);
                        }

                    case ExtendedMemberInitExpression extendedMemberInitExpression:
                        {
                            return VisitExtendedMemberInit(extendedMemberInitExpression);
                        }

                    default:
                        {
                            return base.VisitExtension(node);
                        }
                }
            }

            protected override Expression VisitNew(NewExpression node)
            {
                return node.Update(node.Arguments.Select(MaybeToList));
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

            protected virtual Expression VisitExtendedNew(ExtendedNewExpression node)
            {
                return node.Update(node.Arguments.Select(MaybeToList));
            }

            protected virtual Expression VisitExtendedMemberInit(ExtendedMemberInitExpression node)
            {
                return node.Update(
                    VisitAndConvert(node.NewExpression, nameof(VisitExtendedMemberInit)),
                    node.Arguments.Select(MaybeToList));
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
        }

        private class MemberPathMapping : IEquatable<MemberPathMapping>
        {
            public ParameterExpression Parameter { get; }

            public List<MemberInfo> SourcePath { get; }

            public List<MemberInfo> TargetPath { get; }

            public MemberPathMapping(
                ParameterExpression parameter,
                List<MemberInfo> sourcePath,
                List<MemberInfo> targetPath)
            {
                Parameter = parameter;
                SourcePath = sourcePath;
                TargetPath = targetPath;
            }

            public bool Equals(MemberPathMapping other)
            {
                return Parameter.Equals(other.Parameter)
                    && SourcePath.SequenceEqual(other.SourcePath)
                    && TargetPath.SequenceEqual(other.TargetPath);
            }

            public override string ToString()
            {
                var target = string.Join(".", TargetPath.Select(m => m.Name).Prepend("$R"));
                var source = string.Join(".", SourcePath.Select(m => m.Name).Prepend("$P"));

                return $"{target.PadRight(50)} => {source.PadRight(50)}";
            }
        }

        private sealed class SelectorGatheringExpressionVisitor : ProjectionExpressionVisitor
        {
            private readonly IEnumerable<NavigationDescriptor> descriptors;

            public HashSet<MemberPathMapping> Mappings { get; } = new HashSet<MemberPathMapping>();

            public SelectorGatheringExpressionVisitor(IEnumerable<NavigationDescriptor> descriptors)
            {
                this.descriptors = descriptors;
            }

            protected override Expression VisitLeaf(Expression node)
            {
                switch (node)
                {
                    case MemberExpression memberExpression:
                        {
                            var descriptor = descriptors.SingleOrDefault(d => d.Member == memberExpression.Member);

                            if (descriptor != null)
                            {
                                UnwindMemberExpression(memberExpression, out var expression, out var path);

                                if (expression is ParameterExpression parameterExpression)
                                {
                                    Mappings.Add(
                                        new MemberPathMapping(
                                            parameterExpression,
                                            path,
                                            CurrentPath.ToList()));
                                }
                            }

                            return node;
                        }

                    case ParameterExpression parameterExpression:
                        {
                            Mappings.Add(
                                new MemberPathMapping(
                                    parameterExpression,
                                    new List<MemberInfo>(),
                                    CurrentPath.ToList()));

                            return node;
                        }

                    default:
                        {
                            return node;
                        }
                }
            }
        }
    }
}