using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Rewriting;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query
{
    public class DefaultImpatientExpressionVisitorProvider : IImpatientExpressionVisitorProvider
    {
        private static readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor
            = new TranslatabilityAnalyzingExpressionVisitor();

        public IEnumerable<ExpressionVisitor> RewritingExpressionVisitors { get; } = new ExpressionVisitor[]
        {
            new NullableMemberRewritingExpressionVisitor(),
            new DateTimeMemberRewritingExpressionVisitor(),
            new StringMemberRewritingExpressionVisitor(),
            new CollectionContainsRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor),
            new EnumerableContainsRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor),
        };

        public IEnumerable<ExpressionVisitor> OptimizingExpressionVisitors { get; } = new ExpressionVisitor[]
        {
            new SelectorPushdownExpressionVisitor(),
            new BooleanOptimizingExpressionVisitor(),
        };

        public QueryTranslatingExpressionVisitor QueryTranslatingExpressionVisitor 
            => new QueryTranslatingExpressionVisitor(this);

        public TranslatabilityAnalyzingExpressionVisitor TranslatabilityAnalyzingExpressionVisitor
            => translatabilityAnalyzingExpressionVisitor;

        private List<PrimaryKeyDescriptor> primaryKeyDescriptors = new List<PrimaryKeyDescriptor>();
        private List<NavigationDescriptor> navigationDescriptors = new List<NavigationDescriptor>();

        public DefaultImpatientExpressionVisitorProvider WithPrimaryKeyDescriptors(
            IEnumerable<PrimaryKeyDescriptor> primaryKeyDescriptors)
        {
            this.primaryKeyDescriptors.AddRange(primaryKeyDescriptors);

            return this;
        }

        public DefaultImpatientExpressionVisitorProvider WithNavigationDescriptors(
            IEnumerable<NavigationDescriptor> navigationDescriptors)
        {
            this.navigationDescriptors.AddRange(navigationDescriptors);

            return this;
        }

        public IEnumerable<ExpressionVisitor> MidOptimizationExpressionVisitors
        {
            get
            {
                yield return new KeyEqualityRewritingExpressionVisitor(primaryKeyDescriptors, navigationDescriptors);
                yield return new NavigationRewritingExpressionVisitor(navigationDescriptors);
            }
        }
    }
}
