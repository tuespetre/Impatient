using Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Composing;
using Impatient.Query.ExpressionVisitors.Rewriting;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EFCoreComposingExpressionVisitorProvider : IComposingExpressionVisitorProvider
    {
        private readonly ICurrentDbContext currentDbContext;
        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor;
        private readonly IRewritingExpressionVisitorProvider rewritingExpressionVisitorProvider;

        public EFCoreComposingExpressionVisitorProvider(
            ICurrentDbContext currentDbContext,
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor, 
            IRewritingExpressionVisitorProvider rewritingExpressionVisitorProvider)
        {
            this.currentDbContext = currentDbContext;
            this.translatabilityAnalyzingExpressionVisitor = translatabilityAnalyzingExpressionVisitor;
            this.rewritingExpressionVisitorProvider = rewritingExpressionVisitorProvider;
        }

        public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            // Before any composition, extract the 'query options' 
            // (AsTracking, AsNoTracking, IgnoreQueryFilters)

            yield return new QueryOptionsAnnotatingExpressionVisitor();

            // This visitor ensures that EF.Property calls to non-shadow properties/navigations
            // are rewritten into plain member accesses that the navigation visitor can rewrite.

            yield return new ShadowPropertyRewritingExpressionVisitor(currentDbContext.Context.Model);

            // The include composing visitor rewrites the calls to Include
            // into calls to Select that re-materialize the entity while assigning
            // a navigation property to themselves, so that the navigation composing
            // visitor can then rewrite those into appropriate joins/etc.

            yield return new NavigationComposingExpressionVisitor(context.DescriptorSet.NavigationDescriptors);

            yield return new IncludeComposingExpressionVisitor(currentDbContext.Context.Model, context.DescriptorSet);

            yield return new NavigationComposingExpressionVisitor(context.DescriptorSet.NavigationDescriptors);

            yield return new TableAliasComposingExpressionVisitor();

            // Now that the really 'meaty' compositions have taken place,
            // go back with the query options and apply them (by removing
            // query filter expressions, setting the EntityState to use 
            // within materialization expressions, etc.

            yield return new QueryOptionsComposingExpressionVisitor();

            // Compose the actual relational query from the modified tree

            yield return new QueryComposingExpressionVisitor(
                translatabilityAnalyzingExpressionVisitor,
                rewritingExpressionVisitorProvider.CreateExpressionVisitors(context),
                new SqlParameterRewritingExpressionVisitor(context.ParameterMapping.Values));

            // Apply possible relational null semantics after the whole query is composed
            // but before it is compiled

            yield return new RelationalNullSemanticsComposingExpressionVisitor();
        }
    }
}
