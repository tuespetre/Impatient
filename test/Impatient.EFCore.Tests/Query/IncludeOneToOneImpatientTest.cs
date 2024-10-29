using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class IncludeOneToOneImpatientTest : IncludeOneToOneTestBase<OneToOneQueryImpatientFixture>
{
    public IncludeOneToOneImpatientTest(OneToOneQueryImpatientFixture fixture) : base(fixture)
    {
    }

    [Fact(Skip = "Include using EF.Property is weird. Not going to support it.")]
    public override void Include_address_EF_Property()
    {
        base.Include_address_EF_Property();
    }

    [Fact(Skip = "Include using EF.Property is weird. Not going to support it.")]
    public override void Include_address_no_tracking_EF_Property()
    {
        base.Include_address_no_tracking_EF_Property();
    }

    [Fact(Skip = "Include using EF.Property is weird. Not going to support it.")]
    public override void Include_person_EF_Property()
    {
        base.Include_person_EF_Property();
    }

    [Fact(Skip = "Include using EF.Property is weird. Not going to support it.")]
    public override void Include_person_no_tracking_EF_Property()
    {
        base.Include_person_no_tracking_EF_Property();
    }
}

public class OneToOneQueryImpatientFixture : IncludeOneToOneTestBase<OneToOneQueryImpatientFixture>.OneToOneQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
}
