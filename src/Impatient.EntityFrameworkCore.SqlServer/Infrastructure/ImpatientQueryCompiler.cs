using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Extensions;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure;

public partial class ImpatientQueryCompiler : IQueryCompiler
{
    private readonly ICurrentDbContext currentDbContext;
    private IImpatientQueryProcessor queryProcessor;
    private IAsyncQueryProvider queryProvider;

    public ImpatientQueryCompiler(ICurrentDbContext currentDbContext)
    {
        this.currentDbContext = currentDbContext;
    }

    TResult IQueryCompiler.Execute<TResult>(Expression query)
    {
        var processor = GetQueryProcessor();

        var provider = GetQueryProvider();

        var preparedQuery = PrepareQuery(query);

        return (TResult)processor.Execute(provider, preparedQuery);
    }

    TResult IQueryCompiler.ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
    {
        var processor = GetQueryProcessor();

        var provider = GetQueryProvider();

        var preparedQuery = PrepareQuery(query);

        var result = processor.Execute(provider, preparedQuery);

        if (typeof(TResult).IsGenericType(typeof(IAsyncEnumerable<>)))
        {
            return (TResult)GetType()
                .GetMethod(nameof(ReturnAsyncEnumerable), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(typeof(TResult).GetSequenceType())
                .Invoke(null, [result]);
        }

        if (typeof(TResult).IsGenericType(typeof(ValueTask<>)))
        {
            return (TResult)Activator.CreateInstance(typeof(TResult), result);
        }

        if (typeof(TResult).IsGenericType(typeof(Task<>)))
        {
            return (TResult)typeof(Task)
                .GetMethod(nameof(Task<object>.FromResult))
                .MakeGenericMethod(typeof(TResult).GenericTypeArguments[0])
                .Invoke(null, [result]);
        }

        throw new NotSupportedException("lol");
    }

    private static async IAsyncEnumerable<T> ReturnAsyncEnumerable<T>(IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            yield return item;
        }
    }

    public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
    {
        var provider = GetQueryProvider();

        var context
            = currentDbContext.Context
                .GetService<IQueryProcessingContextFactory>()
                .CreateQueryProcessingContext(provider);

        var visited = ApplyVisitors(query, context);

        var discoverer = new FreeVariableDiscoveringExpressionVisitor();

        discoverer.Visit(visited);

        var discovered = discoverer.DiscoveredVariables.ToArray();

        var parameters = new ParameterExpression[context.ParameterMapping.Count + 1 + discovered.Length];

        parameters[0] = ExecutionContextParameters.DbCommandExecutor;

        context.ParameterMapping.Values.CopyTo(parameters, 1);

        discovered.CopyTo(parameters, 2);

        var parameterArray = Expression.Parameter(typeof(object[]));

        // Wrap the actual lambda in a static invocation.
        // This is faster than just compiling it and calling DynamicInvoke.

        var compiled = Expression
            .Lambda<Func<object[], object>>(
                Expression.Convert(
                    Expression.Invoke(
                        Expression.Lambda(visited, parameters),
                        parameters.Select((p, i) =>
                            Expression.Convert(
                            Expression.ArrayIndex(
                                parameterArray,
                                Expression.Constant(i)),
                                p.Type))),
                    typeof(object)),
                parameterArray)
            .Compile();

        return (queryContext) =>
        {
            var arguments = new object[2 + discovered.Length];

            arguments[0] = queryContext.Context.GetService<IDbCommandExecutorFactory>().Create();
            arguments[1] = queryContext.Context;

            for (var i = 0; i < discovered.Length; i++)
            {
                arguments[i + 2] = queryContext.ParameterValues[discovered[i].Name];
            }

            try
            {
                var result = compiled(arguments);

                return (TResult)result;
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw targetInvocationException.InnerException;
            }
        };
    }

    public Func<QueryContext, TResult> CreateCompiledAsyncQuery<TResult>(Expression query)
    {
        var compiled = CreateCompiledQuery<TResult>(query);

        return (queryContext) =>
        {
            // TODO: wtf?
            return compiled(queryContext);
        };
    }

    private Expression ApplyVisitors(Expression query, QueryProcessingContext context)
    {
        var inlined
               = currentDbContext.Context
                   .GetService<IQueryableInliningExpressionVisitorFactory>()
                   .Create(context).Visit(query);

        var visited = inlined;

        var composingExpressionVisitors
            = currentDbContext.Context
                .GetService<IComposingExpressionVisitorProvider>()
                .CreateExpressionVisitors(context)
                .ToArray();

        var optimizingExpressionVisitors
            = currentDbContext.Context
                .GetService<IOptimizingExpressionVisitorProvider>()
                .CreateExpressionVisitors(context)
                .ToArray();

        var compilingExpressionVisitors
            = currentDbContext.Context
                .GetService<ICompilingExpressionVisitorProvider>()
                .CreateExpressionVisitors(context)
                .ToArray();

        // Apply all optimizing visitors before each composing visitor and then apply all
        // optimizing visitors one last time.

        foreach (var optimizingVisitor in optimizingExpressionVisitors)
        {
            visited = optimizingVisitor.Visit(visited);
        }

        foreach (var composingVisitor in composingExpressionVisitors)
        {
            visited = composingVisitor.Visit(visited);

            foreach (var optimizingVisitor in optimizingExpressionVisitors)
            {
                visited = optimizingVisitor.Visit(visited);
            }
        }

        // Transform the expression by rewriting all composed query expressions into 
        // executable expressions that make database calls and perform result materialization.

        foreach (var compilingVisitor in compilingExpressionVisitors)
        {
            visited = compilingVisitor.Visit(visited);
        }

        return visited;
    }

    private Expression PrepareQuery(Expression query)
    {
        return new QueryOptionsExpression(
            query,
            currentDbContext.Context.ChangeTracker.QueryTrackingBehavior,
            false,
            RelationalOptionsExtension.Extract(
                currentDbContext.Context.GetService<IDbContextOptions>())
                .UseRelationalNulls);
    }

    private IImpatientQueryProcessor GetQueryProcessor()
    {
        return queryProcessor
            ??= ((IInfrastructure<IServiceProvider>)currentDbContext.Context)
                    .Instance.GetRequiredService<IImpatientQueryProcessor>();
    }

    private IAsyncQueryProvider GetQueryProvider()
    {
        return queryProvider ??= currentDbContext.GetDependencies().QueryProvider;
    }
}
