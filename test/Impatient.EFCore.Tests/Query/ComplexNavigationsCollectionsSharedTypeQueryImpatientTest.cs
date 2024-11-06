using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;
using static Impatient.EFCore.Tests.Query.ComplexNavigationsCollectionsSharedTypeQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsCollectionsSharedTypeQueryImpatientTest : ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<Fixture>
{
    public ComplexNavigationsCollectionsSharedTypeQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_after_Select(bool async)
    {
        return base.Include_after_Select(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_after_SelectMany_and_reference_navigation(bool async)
    {
        return base.Include_after_SelectMany_and_reference_navigation(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_collection_with_multiple_orderbys_complex(bool async)
    {
        return base.Include_collection_with_multiple_orderbys_complex(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_collection_with_multiple_orderbys_complex_repeated(bool async)
    {
        return base.Include_collection_with_multiple_orderbys_complex_repeated(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_collection_with_multiple_orderbys_complex_repeated_checked(bool async)
    {
        return base.Include_collection_with_multiple_orderbys_complex_repeated_checked(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_collection_with_multiple_orderbys_member(bool async)
    {
        return base.Include_collection_with_multiple_orderbys_member(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_collection_with_multiple_orderbys_methodcall(bool async)
    {
        return base.Include_collection_with_multiple_orderbys_methodcall(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_collection_with_multiple_orderbys_property(bool async)
    {
        return base.Include_collection_with_multiple_orderbys_property(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_inside_subquery(bool async)
    {
        return base.Include_inside_subquery(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task SelectMany_over_conditional_empty_source(bool async)
    {
        return base.SelectMany_over_conditional_empty_source(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(bool async)
    {
        return base.SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(async);
    }
}