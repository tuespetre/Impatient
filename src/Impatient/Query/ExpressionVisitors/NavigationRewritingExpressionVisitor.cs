using Impatient.Metadata;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors
{
    public class NavigationRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly IEnumerable<NavigationDescriptor> navigationDescriptors;

        public NavigationRewritingExpressionVisitor(IEnumerable<NavigationDescriptor> navigationDescriptors)
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
            private static readonly MethodInfo queryableSelectMethodInfo
                = ImpatientExtensions.GetGenericMethodDefinition((IQueryable<object> o) => o.Select(x => x));

            private static readonly MethodInfo enumerableSelectMethodInfo
                = ImpatientExtensions.GetGenericMethodDefinition((IEnumerable<object> o) => o.Select(x => x));

            private static readonly MethodInfo queryableWhereMethodInfo
                = ImpatientExtensions.GetGenericMethodDefinition((IQueryable<bool> o) => o.Where(x => x));

            private static readonly MethodInfo enumerableWhereMethodInfo
                = ImpatientExtensions.GetGenericMethodDefinition((IEnumerable<bool> o) => o.Where(x => x));

            private readonly IEnumerable<NavigationDescriptor> navigationDescriptors;

            public CoreNavigationRewritingExpressionVisitor(IEnumerable<NavigationDescriptor> navigationDescriptors)
            {
                this.navigationDescriptors = navigationDescriptors ?? throw new ArgumentNullException(nameof(navigationDescriptors));
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if ((node.Method.DeclaringType != typeof(Queryable)
                        && node.Method.DeclaringType != typeof(Enumerable))
                    || node.ContainsNonLambdaDelegates())
                {
                    return base.VisitMethodCall(node);
                }

                var isQueryable = node.Method.DeclaringType == typeof(Queryable);

                var terminalSelectMethod = isQueryable ? queryableSelectMethodInfo : enumerableSelectMethodInfo;

                var terminalWhereMethod = isQueryable ? queryableWhereMethodInfo : queryableSelectMethodInfo;

                Expression Quote(LambdaExpression lambda)
                {
                    return isQueryable ? Expression.Quote(lambda) : lambda as Expression;
                }

                switch (node.Method.Name)
                {
                    // Uncertain
                    case nameof(Queryable.Aggregate):
                    {
                        break;
                    }

                    // TODO: Implement ToDictionary/ToLookup navigation rewriting
                    case nameof(Enumerable.ToDictionary):
                    case nameof(Enumerable.ToLookup):
                    {
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
                    {
                        var source = Visit(node.Arguments[0]);

                        if (source is NavigationExpansionContextExpression nece)
                        {
                            var result
                                = Expression.Call(
                                    node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                        nece.Source.Type.GetSequenceType()),
                                    node.Arguments.Skip(1).Prepend(nece.Source));

                            var terminator
                                = Expression.Call(
                                    terminalSelectMethod.MakeGenericMethod(
                                        result.Type.GetSequenceType(),
                                        node.Type.GetSequenceType()),
                                    result,
                                    Quote(nece.Context.OuterTerminalSelector));

                            return new NavigationExpansionContextExpression(result, terminator, nece.Context);
                        }
                        else
                        {
                            return node.Update(null, node.Arguments.Skip(1).Prepend(source));
                        }
                    }

                    // Complex selector lambdas
                    case nameof(Queryable.Zip):
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
                                = Expression.Call(
                                    node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                        outerSource.Type.GetSequenceType(),
                                        innerSource.Type.GetSequenceType(),
                                        resultSelector.ReturnType),
                                    new[]
                                    {
                                        outerSource,
                                        innerSource,
                                        Quote(resultSelector),
                                    });

                            var terminator
                                = Expression.Call(
                                    terminalSelectMethod.MakeGenericMethod(
                                        resultSelector.ReturnType,
                                        node.Type.GetSequenceType()),
                                    result,
                                    Quote(resultContext.OuterTerminalSelector));

                            return new NavigationExpansionContextExpression(result, terminator, resultContext);
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

                    case nameof(Queryable.GroupBy):
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

                        var parameters = node.Method.GetParameters();
                        var hasElementSelector = parameters.Any(p => p.Name == "elementSelector");
                        var hasResultSelector = parameters.Any(p => p.Name == "resultSelector");

                        if (hasElementSelector && hasResultSelector)
                        {
                            var elementSelector = Visit(node.Arguments[2]).UnwrapLambda();
                            var elementParameter = elementSelector.Parameters[0];
                            var resultSelector = Visit(node.Arguments[3]).UnwrapLambda();

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
                                = Expression.Call(
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
                                        Quote(keySelector),
                                        Quote(elementSelector),
                                        Quote(intermediateResultSelector),
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
                                        Quote(newContext.OuterTerminalSelector));

                                return new NavigationExpansionContextExpression(result, terminator, newContext);
                            }
                            else if (expandedAny)
                            {
                                return Expression.Call(
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
                                        Quote(keySelector),
                                        Quote(elementSelector),
                                        Quote(resultSelector),
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
                        else if (hasElementSelector)
                        {
                            var elementSelector = Visit(node.Arguments[2]).UnwrapLambda();
                            var elementParameter = elementSelector.Parameters[0];

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
                        else if (hasResultSelector)
                        {
                            var expandedKeySelector
                                = context.ExpandIntermediateLambda(
                                    ref source,
                                    ref keySelector,
                                    ref keyParameter,
                                    out _);

                            var resultSelector = Visit(node.Arguments[2]).UnwrapLambda();
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
                                = Expression.Call(
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
                                        Quote(keySelector),
                                        Quote(context.OuterTerminalSelector),
                                        Quote(intermediateResultSelector),
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
                                        Quote(newContext.OuterTerminalSelector));

                                return new NavigationExpansionContextExpression(result, terminator, newContext);
                            }
                            else if (expandedKeySelector)
                            {
                                return Expression.Call(
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
                                        Quote(keySelector),
                                        Quote(context.OuterTerminalSelector),
                                        Quote(resultSelector),
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
                        else
                        {
                            if (context.ExpandIntermediateLambda(ref source, ref keySelector, ref keyParameter, out _))
                            {
                                return Expression.Call(
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
                                        Quote(keySelector),
                                        Quote(context.OuterTerminalSelector),
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
                    }

                    case nameof(Queryable.GroupJoin):
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
                                = Expression.Call(
                                    node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                        outerSource.Type.GetSequenceType(),
                                        innerSource.Type.GetSequenceType(),
                                        outerKeySelector.ReturnType,
                                        resultSelector.ReturnType),
                                    new[]
                                    {
                                        outerSource,
                                        innerSource,
                                        Quote(outerKeySelector),
                                        Quote(innerKeySelector),
                                        Quote(resultSelector),
                                    });

                            var terminator
                                = Expression.Call(
                                    terminalSelectMethod.MakeGenericMethod(
                                        resultSelector.ReturnType,
                                        node.Type.GetSequenceType()),
                                    result,
                                    Quote(outerContext.OuterTerminalSelector));

                            return new NavigationExpansionContextExpression(result, terminator, outerContext);
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

                    case nameof(Queryable.Join):
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
                                = Expression.Call(
                                    node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                        outerSource.Type.GetSequenceType(),
                                        innerSource.Type.GetSequenceType(),
                                        outerKeySelector.ReturnType,
                                        resultSelector.ReturnType),
                                    new[]
                                    {
                                        outerSource,
                                        innerSource,
                                        Quote(outerKeySelector),
                                        Quote(innerKeySelector),
                                        Quote(resultSelector),
                                    });

                            var terminator
                                = Expression.Call(
                                    terminalSelectMethod.MakeGenericMethod(
                                        resultSelector.ReturnType,
                                        node.Type.GetSequenceType()),
                                    result,
                                    Quote(resultContext.OuterTerminalSelector));

                            return new NavigationExpansionContextExpression(result, terminator, resultContext);
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

                    case nameof(Queryable.Select):
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
                                = Expression.Call(
                                    node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                        parameter.Type,
                                        selector.ReturnType),
                                    source,
                                    Quote(selector));

                            var terminator
                                = Expression.Call(
                                    terminalSelectMethod.MakeGenericMethod(
                                        selector.ReturnType,
                                        node.Type.GetSequenceType()),
                                    result,
                                    Quote(context.OuterTerminalSelector));

                            return new NavigationExpansionContextExpression(result, terminator, context);
                        }
                        else
                        {
                            return node.Update(null, new[] { source, selector }.Concat(node.Arguments.Skip(2)));
                        }
                    }

                    case nameof(Queryable.SelectMany)
                    when node.Arguments.Count == 2:
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
                            return Expression.Call(
                                node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                    parameter.Type,
                                    node.Method.GetGenericArguments()[1]),
                                source,
                                Quote(selector));
                        }
                        else
                        {
                            return node.Update(null, new[] { source, selector }.Concat(node.Arguments.Skip(2)));
                        }
                    }

                    case nameof(Queryable.SelectMany)
                    when node.Arguments.Count == 3:
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
                            var result
                                = Expression.Call(
                                    node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                        parameter1.Type,
                                        node.Method.GetGenericArguments()[1],
                                        resultSelector.ReturnType),
                                    source,
                                    Quote(collectionSelector),
                                    Quote(resultSelector));

                            var terminator
                                = Expression.Call(
                                    terminalSelectMethod.MakeGenericMethod(
                                        resultSelector.ReturnType,
                                        node.Type.GetSequenceType()),
                                    result,
                                    Quote(context.OuterTerminalSelector));

                            return new NavigationExpansionContextExpression(result, terminator, context);
                        }
                        else
                        {
                            return node.Update(null, new[] { source, collectionSelector, resultSelector }.Concat(node.Arguments.Skip(3)));
                        }
                    }

                    // Intermediate selector lambdas
                    case nameof(Queryable.OrderBy):
                    case nameof(Queryable.OrderByDescending):
                    case nameof(Queryable.ThenBy):
                    case nameof(Queryable.ThenByDescending):
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
                                = Expression.Call(
                                    node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                        parameter.Type,
                                        node.Method.GetGenericArguments()[1]),
                                    source,
                                    Quote(selector));

                            var terminator
                                = Expression.Call(
                                    terminalSelectMethod.MakeGenericMethod(
                                        parameter.Type,
                                        node.Type.GetSequenceType()),
                                    result,
                                    Quote(context.OuterTerminalSelector));

                            return new NavigationExpansionContextExpression(result, terminator, context);
                        }
                        else
                        {
                            return node.Update(null, new[] { source, selector }.Concat(node.Arguments.Skip(2)));
                        }
                    }

                    // Terminal selector lambdas
                    case nameof(Queryable.Average) when node.Arguments.Count == 2:
                    case nameof(Queryable.Max) when node.Arguments.Count == 2:
                    case nameof(Queryable.Min) when node.Arguments.Count == 2:
                    case nameof(Queryable.Sum) when node.Arguments.Count == 2:
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
                            return Expression.Call(
                                node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                    node.Method.GetGenericArguments()
                                        .Skip(1)
                                        .Prepend(parameter.Type)
                                        .ToArray()),
                                source,
                                Quote(selector));
                        }
                        else
                        {
                            return node.Update(null, new[] { source, selector }.Concat(node.Arguments.Skip(2)));
                        }
                    }

                    // Type-independent terminal predicate lambdas
                    case nameof(Queryable.All):
                    case nameof(Queryable.Any) when node.Arguments.Count == 2:
                    case nameof(Queryable.Count) when node.Arguments.Count == 2:
                    case nameof(Queryable.LongCount) when node.Arguments.Count == 2:
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
                            return Expression.Call(
                                node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                    parameter.Type),
                                source,
                                Quote(predicate));
                        }
                        else
                        {
                            return node.Update(null, new[] { source, predicate }.Concat(node.Arguments.Skip(2)));
                        }
                    }

                    // Type-dependent terminal predicate lambdas
                    case nameof(Queryable.First) when node.Arguments.Count == 2:
                    case nameof(Queryable.FirstOrDefault) when node.Arguments.Count == 2:
                    case nameof(Queryable.Last) when node.Arguments.Count == 2:
                    case nameof(Queryable.LastOrDefault) when node.Arguments.Count == 2:
                    case nameof(Queryable.Single) when node.Arguments.Count == 2:
                    case nameof(Queryable.SingleOrDefault) when node.Arguments.Count == 2:
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
                                        Quote(predicate)),
                                    Quote(context.OuterTerminalSelector)));
                        }
                        else
                        {
                            return node.Update(null, new[] { source, predicate }.Concat(node.Arguments.Skip(2)));
                        }
                    }

                    // Non-terminal predicate lambdas
                    case nameof(Queryable.SkipWhile):
                    case nameof(Queryable.TakeWhile):
                    case nameof(Queryable.Where):
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
                                = Expression.Call(
                                    node.Method.GetGenericMethodDefinition().MakeGenericMethod(
                                        parameter.Type),
                                    source,
                                    Quote(predicate));

                            var terminator
                                = Expression.Call(
                                    terminalSelectMethod.MakeGenericMethod(
                                        parameter.Type,
                                        node.Method.GetGenericArguments()[0]),
                                    result,
                                    Quote(context.OuterTerminalSelector));

                            return new NavigationExpansionContextExpression(result, terminator, context);
                        }
                        else
                        {
                            return node.Update(null, new[] { source, predicate }.Concat(node.Arguments.Skip(2)));
                        }
                    }
                }

                return base.VisitMethodCall(node);
            }
        }

        private sealed class NavigationContextTerminatingExpressionVisitor : ExpressionVisitor
        {
            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case NavigationExpansionContextExpression nece:
                    {
                        return Visit(nece.Terminator ?? nece.Source);
                    }

                    default:
                    {
                        return base.Visit(node);
                    }
                }
            }
        }

        private sealed class NavigationExpansionContextExpression : Expression
        {
            public NavigationExpansionContextExpression(Expression source, Expression terminator, NavigationExpansionContext context)
            {
                Source = source ?? throw new ArgumentNullException(nameof(source));
                Terminator = terminator;
                Context = context ?? throw new ArgumentNullException(nameof(context));
            }

            public Expression Source { get; }

            public Expression Terminator { get; }

            public NavigationExpansionContext Context { get; }

            public override ExpressionType NodeType => ExpressionType.Extension;

            public override Type Type => Terminator?.Type ?? Source.Type;

            protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
        }

        private struct NavigationTransparentIdentifier<TOuter, TInner>
        {
            public NavigationTransparentIdentifier(TOuter outer, TInner inner)
            {
                Outer = outer;
                Inner = inner;
            }

            [PathSegmentName(null)]
            public TOuter Outer;

            [PathSegmentName(null)]
            public TInner Inner;
        }

        private sealed class ExpansionMapping
        {
            public List<MemberInfo> OldPath;
            public List<MemberInfo> NewPath;
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
            private List<MemberInfo> terminalPath = new List<MemberInfo>();

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
                    terminalPath
                        .AsEnumerable()
                        .Reverse()
                        .Aggregate(
                            currentParameter as Expression,
                            Expression.MakeMemberAccess),
                    currentParameter);

            public bool ExpandIntermediateLambda(
                ref Expression source,
                ref LambdaExpression lambda,
                ref ParameterExpression parameter,
                out bool foundNavigations)
            {
                if (!(foundNavigations = ProcessNavigations(ref source, ref lambda, ref parameter))
                    && terminalPath.Count == 0)
                {
                    return false;
                }

                var lambdaBody = lambda.Body;

                lambdaBody
                    = new NavigationExpandingExpressionVisitor(parameter, currentParameter, mappings)
                        .Visit(lambdaBody);

                lambda
                    = Expression.Lambda(
                        lambdaBody,
                        SwapParameter(lambda.Parameters, parameter, currentParameter));

                parameter = currentParameter;

                return true;
            }

            public bool ExpandResultLambda(
                ref Expression source,
                ref LambdaExpression lambda,
                ref ParameterExpression parameter)
            {
                if (!ProcessNavigations(ref source, ref lambda, ref parameter)
                    && terminalPath.Count == 0)
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

                var lambdaBody = lambda.Body;

                lambdaBody
                    = new NavigationExpandingExpressionVisitor(parameter, currentParameter, mappings)
                        .Visit(lambdaBody);

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
                terminalPath.Add(innerField);

                mappings.Clear();

                mappings.Add(new ExpansionMapping
                {
                    OldPath = new List<MemberInfo>(),
                    NewPath = new List<MemberInfo> { innerField },
                });

                newMappings.ForEach(m => m.NewPath.Insert(0, outerField));

                mappings.AddRange(newMappings);

                return true;
            }

            private bool ProcessNavigations(
                ref Expression source,
                ref LambdaExpression lambda,
                ref ParameterExpression parameter)
            {
                var findingVisitor = new NavigationFindingExpressionVisitor(parameter, descriptors, mappings);

                findingVisitor.Visit(lambda.Body);

                if (!findingVisitor.FoundNavigations.Any())
                {
                    return false;
                }

                var operatorType
                    = source.Type.GetInterfaces().Contains(typeof(IQueryable))
                        ? typeof(Queryable)
                        : typeof(Enumerable);

                foreach (var navigation in findingVisitor.FoundNavigations.OrderBy(f => f.Path.Count()))
                {
                    var outerType = source.Type.GetSequenceType();
                    var innerType = navigation.DestinationType.FindGenericType(typeof(IEnumerable<>)) ?? navigation.DestinationType;

                    var scopeType
                        = typeof(NavigationTransparentIdentifier<,>)
                            .MakeGenericType(outerType, innerType);

                    var outerField = scopeType.GetRuntimeField("Outer");
                    var innerField = scopeType.GetRuntimeField("Inner");

                    var lastMapping = mappings.Last();

                    var outerKeyPath
                        = lastMapping.NewPath.Concat(
                            navigation.Path.Except(lastMapping.OldPath).SkipLast(1));

                    if (navigation.DestinationType.IsSequenceType())
                    {
                        var method
                            = operatorType
                                .GetRuntimeMethods()
                                .Single(m => m.Name == nameof(Queryable.GroupJoin) && m.GetParameters().Length == 5)
                                .MakeGenericMethod(
                                    currentParameter.Type,
                                    innerType.GetSequenceType(),
                                    navigation.Descriptor.OuterKeySelector.ReturnType,
                                    scopeType);

                        // Expand mappings into the outer key selector
                        var outerKeySelector
                            = Expression.Lambda(
                                navigation.Descriptor.OuterKeySelector.Body.Replace(
                                    navigation.Descriptor.OuterKeySelector.Parameters.Single(),
                                    outerKeyPath.Aggregate(
                                        currentParameter as Expression,
                                        Expression.MakeMemberAccess)),
                                currentParameter);

                        var innerKeySelector
                            = navigation.Descriptor.InnerKeySelector;

                        var innerParameter
                            = Expression.Parameter(innerType, innerKeySelector.Parameters.Single().Name);

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
                                navigation.Descriptor.Expansion,
                                outerKeySelector,
                                innerKeySelector,
                                resultSelector);
                    }
                    else if (navigation.Descriptor.IsNullable)
                    {
                        // TODO: Handle nullable/optional navigations
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var method
                            = operatorType
                                .GetRuntimeMethods()
                                .Single(m => m.Name == nameof(Queryable.Join) && m.GetParameters().Length == 5)
                                .MakeGenericMethod(
                                    currentParameter.Type,
                                    innerType,
                                    navigation.Descriptor.OuterKeySelector.ReturnType,
                                    scopeType);

                        // Expand mappings into the outer key selector
                        var outerKeySelector
                            = Expression.Lambda(
                                navigation.Descriptor.OuterKeySelector.Body.Replace(
                                    navigation.Descriptor.OuterKeySelector.Parameters.Single(),
                                    outerKeyPath.Aggregate(
                                        currentParameter as Expression,
                                        Expression.MakeMemberAccess)),
                                currentParameter);

                        var innerKeySelector
                            = navigation.Descriptor.InnerKeySelector;

                        var innerParameter
                            = Expression.Parameter(innerType, innerKeySelector.Parameters.Single().Name);

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
                                navigation.Descriptor.Expansion,
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
                    });

                    terminalPath.Add(outerField);
                }

                return true;
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
                    lambdaBody
                        = new NavigationExpandingExpressionVisitor(
                            lambda.Parameters[0], outerParameter, outerContext.mappings)
                                .Visit(lambdaBody);

                    lambdaBody
                        = new NavigationExpandingExpressionVisitor(
                            lambda.Parameters[1], innerParameter, innerContext.mappings)
                                .Visit(lambdaBody);
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
                resultContext.terminalPath.Add(resultInnerField);

                return resultContext;
            }
        }

        private static void UnwindMemberExpression(
            MemberExpression memberExpression,
            out Expression innerExpression,
            out List<MemberInfo> path)
        {
            innerExpression = memberExpression.Expression;

            path = new List<MemberInfo>
            {
                memberExpression.Member
            };

            while (innerExpression is MemberExpression innerMemberExpression)
            {
                path.Add(innerMemberExpression.Member);
                innerExpression = innerMemberExpression.Expression;
            }

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
                var descriptor = descriptors.Cast<NavigationDescriptor>().SingleOrDefault(d => d.Member == node.Member);

                if (descriptor == null)
                {
                    return base.VisitMember(node);
                }

                UnwindMemberExpression(node, out var expression, out var path);

                if (expression == targetParameter)
                {
                    if (!mappings.Any(m => m.OldPath.SequenceEqual(path))
                        && !FoundNavigations.Any(f => f.Path.SequenceEqual(path)))
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

                return base.VisitMember(node);
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
                        UnwindMemberExpression(memberExpression, out var expression, out var path);

                        if (expression == oldParameter)
                        {
                            foreach (var mapping in mappings)
                            {
                                if (mapping.OldPath.Take(path.Count).SequenceEqual(path))
                                {
                                    NewMappings.Add(new ExpansionMapping
                                    {
                                        OldPath = CurrentPath.Concat(mapping.OldPath.Skip(path.Count)).ToList(),
                                        NewPath = mapping.NewPath,
                                    });
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
    }
}
