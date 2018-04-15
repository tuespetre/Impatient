using Impatient.Query.ExpressionVisitors.Optimizing;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    /// <summary>
    /// A default implementation of <see cref="IOptimizingExpressionVisitorProvider"/>
    /// that supplies a <see cref="SelectorPushdownExpressionVisitor"/> and a
    /// <see cref="BooleanOptimizingExpressionVisitor"/>. This implementation is
    /// safe to register as a singleton service in a service container.
    /// </summary>
    public class DefaultOptimizingExpressionVisitorProvider : IOptimizingExpressionVisitorProvider
    {
        public IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            yield return new SelectorPushdownExpressionVisitor();

            yield return new BooleanOptimizingExpressionVisitor();

            yield return new RedundantConversionStrippingExpressionVisitor();
        }
    }
}
