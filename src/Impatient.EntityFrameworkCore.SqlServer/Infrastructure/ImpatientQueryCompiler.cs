using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Query;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    // TODO: Make use of the execution strategy
    public partial class ImpatientQueryCompiler : IQueryCompiler
    {
        private readonly ICurrentDbContext currentDbContext;
        private IImpatientQueryExecutor queryExecutor;

        public ImpatientQueryCompiler(ICurrentDbContext currentDbContext)
        {
            this.currentDbContext = currentDbContext;
        }

        public TResult Execute<TResult>(Expression query)
        {
            return (TResult)GetQueryExecutor().Execute(
                currentDbContext.GetDependencies().QueryProvider,
                PrepareQuery(query));
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
        {
            // TODO: Proper async support

            return new BadAsyncEnumerable<TResult>(async () =>
            {
                var enumerable = await ExecuteAsync<IEnumerable<TResult>>(query, default);

                return enumerable.GetEnumerator();
            });
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            // TODO: Proper async support

            return Task.Run(() => Execute<TResult>(query), cancellationToken);
        }

        public Func<QueryContext, IAsyncEnumerable<TResult>> CreateCompiledAsyncEnumerableQuery<TResult>(Expression query)
        {
            ThrowCompiledQueryNotSupported();
            return default;
        }

        public Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query)
        {
            ThrowCompiledQueryNotSupported();
            return default;
        }

        public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            ThrowCompiledQueryNotSupported();
            return default;
        }

        private static void ThrowCompiledQueryNotSupported()
        {
            throw new NotSupportedException("Impatient does not currently support ad-hoc compiled queries.");
        }

        private Expression PrepareQuery(Expression query)
        {
            return new QueryOptionsExpression(query, currentDbContext.Context.ChangeTracker.QueryTrackingBehavior, false);
        }

        private IImpatientQueryExecutor GetQueryExecutor()
        {
            if (queryExecutor == null)
            {
                queryExecutor
                    = ((IInfrastructure<IServiceProvider>)currentDbContext.Context)
                        .Instance.GetRequiredService<IImpatientQueryExecutor>();
            }

            return queryExecutor;
        }
    }

    internal class BadAsyncEnumerable<TResult> : IAsyncEnumerable<TResult>
    {
        private readonly Func<Task<IEnumerator<TResult>>> func;

        public BadAsyncEnumerable(Func<Task<IEnumerator<TResult>>> func)
        {
            this.func = func;
        }

        public IAsyncEnumerator<TResult> GetEnumerator()
        {
            return new BadAsyncEnumerator<TResult>(func());
        }
    }

    internal class BadAsyncEnumerator<TResult> : IAsyncEnumerator<TResult>
    {
        private readonly Task<IEnumerator<TResult>> task;
        private IEnumerator<TResult> enumerator;

        public BadAsyncEnumerator(Task<IEnumerator<TResult>> task)
        {
            this.task = task;
        }

        public TResult Current => enumerator.Current;

        public void Dispose()
        {
            task.Dispose();
            enumerator?.Dispose();
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            if (enumerator == null)
            {
                enumerator = await task;
            }

            return enumerator.MoveNext();
        }
    }
}
