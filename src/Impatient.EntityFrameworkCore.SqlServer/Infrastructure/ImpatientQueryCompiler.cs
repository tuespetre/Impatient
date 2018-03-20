using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Query;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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
            // TODO: Support async

            throw new NotImplementedException();
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken)
        {
            // TODO: Support async

            throw new NotImplementedException();
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
}
