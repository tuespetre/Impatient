using Impatient.Query.ExpressionVisitors.Normalizing;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure;

/// <summary>
/// A default implementation of <see cref="IOptimizingExpressionVisitorProvider"/>
/// that supplies a <see cref="SelectorPushdownExpressionVisitor"/> and a
/// <see cref="BooleanOptimizingExpressionVisitor"/>. This implementation is
/// safe to register as a singleton service in a service container.
/// </summary>
public class DefaultOptimizingExpressionVisitorProvider : IOptimizingExpressionVisitorProvider
{
    private readonly ExpressionTranslatabilityAnalyzer translatabilityAnalyzer;
    private readonly ITypeMappingProvider typeMappingProvider;

    public DefaultOptimizingExpressionVisitorProvider(
        ExpressionTranslatabilityAnalyzer translatabilityAnalyzer,
        ITypeMappingProvider typeMappingProvider)
    {
        this.translatabilityAnalyzer
            = translatabilityAnalyzer
            ?? throw new ArgumentNullException(nameof(translatabilityAnalyzer));

        this.typeMappingProvider
            = typeMappingProvider
            ?? throw new ArgumentNullException(nameof(typeMappingProvider));
    }

    public IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
    {
        // 'proper' optimization

        yield return new TypeBinaryOptimizingExpressionVisitor();

        yield return new ConditionalComparisonOptimizingExpressionVisitor();

        yield return new SelectorPushdownExpressionVisitor();

        yield return new RedundantConversionStrippingExpressionVisitor();

        yield return new NullOrDefaultEqualityOptimizingExpressionVisitor();

        yield return new BooleanOptimizingExpressionVisitor();

        // normalization

        yield return new ListMemberNormalizingExpressionVisitor();

        yield return new CollectionMemberNormalizingExpressionVisitor();

        yield return new EqualsMethodNormalizingExpressionVisitor();

        yield return new NullableMemberNormalizingExpressionVisitor();

        yield return new EnumHasFlagNormalizingExpressionVisitor(typeMappingProvider);

        yield return new StringMemberNormalizingExpressionVisitor();
    }
}
