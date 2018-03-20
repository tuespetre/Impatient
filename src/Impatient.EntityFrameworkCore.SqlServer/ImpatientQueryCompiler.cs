using Impatient.Query;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public partial class ImpatientQueryCompiler : IQueryCompiler
    {
        private readonly ICurrentDbContext currentDbContext;
        private readonly DescriptorSetCache descriptorSetCache;        
        private readonly IImpatientQueryCache queryCache;
        private readonly IDbCommandExecutor dbCommandExecutor;
        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor;
        private readonly IOptimizingExpressionVisitorProvider optimizingExpressionVisitorProvider;
        private readonly IComposingExpressionVisitorProvider composingExpressionVisitorProvider;
        private readonly ICompilingExpressionVisitorProvider compilingExpressionVisitorProvider;
        private readonly IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory;

        public ImpatientQueryCompiler(
            ICurrentDbContext currentDbContext,
            DescriptorSetCache descriptorSetCache,
            IImpatientQueryCache queryCache,
            IDbCommandExecutor dbCommandExecutor,
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor,
            IOptimizingExpressionVisitorProvider optimizingExpressionVisitorProvider,
            IComposingExpressionVisitorProvider composingExpressionVisitorProvider,
            ICompilingExpressionVisitorProvider compilingExpressionVisitorProvider,
            IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory)
        {
            this.currentDbContext = currentDbContext;
            this.descriptorSetCache = descriptorSetCache;            
            this.queryCache = queryCache;
            this.dbCommandExecutor = dbCommandExecutor;
            this.translatabilityAnalyzingExpressionVisitor = translatabilityAnalyzingExpressionVisitor;
            this.optimizingExpressionVisitorProvider = optimizingExpressionVisitorProvider;
            this.composingExpressionVisitorProvider = composingExpressionVisitorProvider;
            this.compilingExpressionVisitorProvider = compilingExpressionVisitorProvider;
            this.queryTranslatingExpressionVisitorFactory = queryTranslatingExpressionVisitorFactory;
        }

        public TResult Execute<TResult>(Expression query)
        {
            // TODO: Make use of the execution strategy

            return (TResult)CreateQueryExecutor(query).Execute(currentDbContext.GetDependencies().QueryProvider, query);
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

        private IImpatientQueryExecutor CreateQueryExecutor(Expression query)
        {
            var model = currentDbContext.Context.Model;
            var queryOptions = GetQueryOptions(query);
            var descriptorSet = descriptorSetCache.GetDescriptorSet(model, queryOptions);
            var queryProvider = currentDbContext.GetDependencies().QueryProvider;
            var queryInliningExpressionVisitorFactory = new EFCoreQueryableInliningExpressionVisitorFactory(model, queryOptions);

            return new DefaultImpatientQueryExecutor(
                descriptorSet,
                queryCache,
                dbCommandExecutor,
                translatabilityAnalyzingExpressionVisitor,
                optimizingExpressionVisitorProvider,
                composingExpressionVisitorProvider,
                compilingExpressionVisitorProvider,
                queryInliningExpressionVisitorFactory,
                queryTranslatingExpressionVisitorFactory);
        }

        private QueryOptions GetQueryOptions(Expression query)
        {
            var optionsVisitor = new EntityQueryableOptionsDiscoveringVisitor();

            optionsVisitor.Visit(query);

            var useTracking = currentDbContext.Context.ChangeTracker.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;

            if (optionsVisitor.FoundAsTracking)
            {
                useTracking = true;
            }
            else if (optionsVisitor.FoundAsNoTracking)
            {
                useTracking = false;
            }

            return new QueryOptions(optionsVisitor.FoundIgnoreQueryFilters, useTracking);
        }
    }
}
