using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Impatient.EFCore.Tests.Query;

public class ChangeTrackingImpatientTest : NorthwindChangeTrackingQueryTestBase<NorthwindQueryImpatientFixture>
{
    public ChangeTrackingImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
    }

    protected override NorthwindContext CreateNoTrackingContext()
        => new ImpatientNorthwindContext(
            new DbContextOptionsBuilder(Fixture.CreateOptions())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);
}
