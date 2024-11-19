using Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure;

public class EFCoreCompilingExpressionVisitorProvider : DefaultCompilingExpressionVisitorProvider
{
    private readonly ICurrentDbContext currentDbContext;

    private static readonly ShadowPropertyRemovingExpressionVisitor shadowPropertyRemovingExpressionVisitor = new();
    private static readonly EntityMaterializationCompilingExpressionVisitor entityMaterializationCompilingExpressionVisitor = new();
    private static readonly IncludeCompilingExpressionVisitor includeCompilingExpressionVisitor = new();

    public EFCoreCompilingExpressionVisitorProvider(
        ICurrentDbContext currentDbContext,
        TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor,
        IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory,
        IReadValueExpressionFactoryProvider readValueExpressionFactoryProvider)
        : base(translatabilityAnalyzingExpressionVisitor,
              queryTranslatingExpressionVisitorFactory,
              readValueExpressionFactoryProvider)
    {
        this.currentDbContext = currentDbContext ?? throw new ArgumentNullException(nameof(currentDbContext));
    }

    public override IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
    {
        // Remove any unneeded shadow properties from the query results before compiling materializers.

        yield return shadowPropertyRemovingExpressionVisitor;

        foreach (var visitor in base.CreateExpressionVisitors(context))
        {
            yield return visitor;
        }

        yield return new ShadowPropertyCompilingExpressionVisitor(currentDbContext.Context.Model);

        yield return entityMaterializationCompilingExpressionVisitor;

        yield return includeCompilingExpressionVisitor;

        // TODO: this
        //yield return new ConcurrencyDetectionCompilingExpressionVisitor();
    }
}