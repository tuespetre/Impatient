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

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public partial class ImpatientQueryCompiler : IQueryCompiler
    {
        private readonly ICurrentDbContext currentDbContext;
        private IImpatientQueryProcessor queryProcessor;
        private IAsyncQueryProvider queryProvider;

        public ImpatientQueryCompiler(ICurrentDbContext currentDbContext)
        {
            this.currentDbContext = currentDbContext;
        }

        public TResult Execute<TResult>(Expression query)
        {
            var processor = GetQueryProcessor();

            var provider = GetQueryProvider();

            var preparedQuery = PrepareQuery(query);

            return (TResult)processor.Execute(provider, preparedQuery);
        }

        /*
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
        {
            return new BadAsyncEnumerable<TResult>(async () =>
            {
                var enumerable = await ExecuteAsync<IEnumerable<TResult>>(query, default);

                return enumerable.GetEnumerator();
            });
        }
        */

        /*
        public Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            return Task.Run(() => Execute<TResult>(query), cancellationToken);
        }
        */

        /*
        public Func<QueryContext, IAsyncEnumerable<TResult>> CreateCompiledAsyncEnumerableQuery<TResult>(Expression query)
        {
            var compiled = CreateCompiledAsyncTaskQuery<IEnumerable<TResult>>(query);

            return (QueryContext queryContext) =>
            {
                return new BadAsyncEnumerable<TResult>(async () =>
                {
                    var enumerable = await compiled(queryContext);

                    return enumerable.GetEnumerator();
                });
            };
        }
        */

        /*
        public Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query)
        {
            var compiled = CreateCompiledQuery<TResult>(query);

            return (QueryContext queryContext) =>
            {
                return Task.Run(() => compiled(queryContext), queryContext.CancellationToken);
            };
        }
        */

        TResult IQueryCompiler.ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            if (typeof(TResult).IsGenericType(typeof(IAsyncEnumerable<>)))
            {
            }

            //var task = Task.Run(() => Execute<TResult>(query), cancellationToken);

            // TODO: something the fuck else than this!
            return Execute<TResult>(query);
        }

        public Func<QueryContext, TResult> CreateCompiledAsyncQuery<TResult>(Expression query)
        {
            var compiled = CreateCompiledQuery<TResult>(query);

            return (QueryContext queryContext) =>
            {
                // TODO: wtf?
                return compiled(queryContext);
            };
        }

        public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            var provider = GetQueryProvider();

            var context 
                = currentDbContext.Context
                    .GetService<IQueryProcessingContextFactory>()
                    .CreateQueryProcessingContext(provider);

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

            return (QueryContext queryContext) =>
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
            if (queryProcessor is null)
            {
                queryProcessor
                    = ((IInfrastructure<IServiceProvider>)currentDbContext.Context)
                        .Instance.GetRequiredService<IImpatientQueryProcessor>();
            }

            return queryProcessor;
        }

        private IAsyncQueryProvider GetQueryProvider()
        {
            if (queryProvider is null)
            {
                queryProvider = currentDbContext.GetDependencies().QueryProvider;
            }

            return queryProvider;
        }
    }
}
