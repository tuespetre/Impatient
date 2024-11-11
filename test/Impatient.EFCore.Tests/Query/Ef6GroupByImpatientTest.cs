using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class Ef6GroupByImpatientTest(Ef6GroupByImpatientTest.Fixture fixture) : Ef6GroupByTestBase<Ef6GroupByImpatientTest.Fixture>(fixture)
{
    [ConditionalTheory(Skip = TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task All_Grouped_from_LINQ_101(bool async)
    {
        return base.All_Grouped_from_LINQ_101(async);
    }

    [ConditionalTheory(Skip = TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task Any_Grouped_from_LINQ_101(bool async)
    {
        return base.Any_Grouped_from_LINQ_101(async);
    }

    [ConditionalTheory(Skip = TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task GroupBy_Nested_from_LINQ_101(bool async)
    {
        return base.GroupBy_Nested_from_LINQ_101(async);
    }

    [ConditionalTheory(Skip = TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task GroupBy_Simple_1_from_LINQ_101(bool async)
    {
        return base.GroupBy_Simple_1_from_LINQ_101(async);
    }

    [ConditionalTheory(Skip = TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task GroupBy_Simple_2_from_LINQ_101(bool async)
    {
        return base.GroupBy_Simple_2_from_LINQ_101(async);
    }

    [ConditionalTheory(Skip = TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task GroupBy_Simple_3_from_LINQ_101(bool async)
    {
        return base.GroupBy_Simple_3_from_LINQ_101(async);
    }

    [ConditionalTheory(Skip = TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task Max_Elements_from_LINQ_101(bool async)
    {
        return base.Max_Elements_from_LINQ_101(async);
    }

    [ConditionalTheory(Skip = TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task Min_Elements_from_LINQ_101(bool async)
    {
        return base.Min_Elements_from_LINQ_101(async);
    }

    [ConditionalTheory(Skip = TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task Whats_new_2021_sample_14(bool async)
    {
        return base.Whats_new_2021_sample_14(async);
    }

    [ConditionalTheory(Skip = TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task Whats_new_2021_sample_16(bool async)
    {
        return base.Whats_new_2021_sample_16(async);
    }

    public new class Fixture : Ef6GroupByFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
