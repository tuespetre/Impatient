using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;
using static Impatient.EFCore.Tests.Query.ComplexNavigationsQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsQueryImpatientTest : ComplexNavigationsQueryRelationalTestBase<Fixture>
{
    public ComplexNavigationsQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : ComplexNavigationsQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [Theory(Skip = EFCoreSkipReasons.BadMaterialization)]
    public override Task Complex_query_with_let_collection_SelectMany(bool async)
    {
        return base.Complex_query_with_let_collection_SelectMany(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Level4_Include(bool async)
    {
        return base.Level4_Include(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Multiple_required_navigation_using_multiple_selects_with_Include(bool async)
    {
        return base.Multiple_required_navigation_using_multiple_selects_with_Include(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Multiple_required_navigation_using_multiple_selects_with_string_based_Include(bool async)
    {
        return base.Multiple_required_navigation_using_multiple_selects_with_string_based_Include(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Multiple_required_navigation_with_string_based_Include(bool async)
    {
        return base.Multiple_required_navigation_with_string_based_Include(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Multiple_required_navigations_with_Include(bool async)
    {
        return base.Multiple_required_navigations_with_Include(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Optional_navigation_with_Include(bool async)
    {
        return base.Optional_navigation_with_Include(async);
    }

    [Theory(Skip = EFCoreSkipReasons.BadMaterialization)]
    public override Task Select_projecting_queryable_followed_by_Join(bool async)
    {
        return base.Select_projecting_queryable_followed_by_Join(async);
    }

    [Theory(Skip = EFCoreSkipReasons.BadMaterialization)]
    public override Task Select_projecting_queryable_followed_by_SelectMany(bool async)
    {
        return base.Select_projecting_queryable_followed_by_SelectMany(async);
    }

    [Theory(Skip = EFCoreSkipReasons.BadMaterialization)]
    public override Task Select_projecting_queryable_in_anonymous_projection_followed_by_Join(bool async)
    {
        return base.Select_projecting_queryable_in_anonymous_projection_followed_by_Join(async);
    }
}