using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class JsonQueryImpatientTest : JsonQueryTestBase<JsonQueryImpatientTest.Fixture>
{
    public JsonQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : JsonQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [ConditionalTheory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task FromSql_on_entity_with_json_basic(bool async)
    {
        return base.FromSql_on_entity_with_json_basic(async);
    }

    [ConditionalTheory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task FromSql_on_entity_with_json_inheritance_on_base(bool async)
    {
        return base.FromSql_on_entity_with_json_inheritance_on_base(async);
    }

    [ConditionalTheory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task FromSql_on_entity_with_json_inheritance_on_derived(bool async)
    {
        return base.FromSql_on_entity_with_json_inheritance_on_derived(async);
    }

    [ConditionalTheory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task FromSql_on_entity_with_json_inheritance_project_reference_on_base(bool async)
    {
        return base.FromSql_on_entity_with_json_inheritance_project_reference_on_base(async);
    }

    [ConditionalTheory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task FromSql_on_entity_with_json_inheritance_project_reference_on_derived(bool async)
    {
        return base.FromSql_on_entity_with_json_inheritance_project_reference_on_derived(async);
    }

    [ConditionalTheory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task FromSql_on_entity_with_json_project_json_collection(bool async)
    {
        return base.FromSql_on_entity_with_json_project_json_collection(async);
    }

    [ConditionalTheory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task FromSql_on_entity_with_json_project_json_reference(bool async)
    {
        return base.FromSql_on_entity_with_json_project_json_reference(async);
    }

    public override Task Json_predicate_on_single(bool async)
    {
        return base.Json_predicate_on_single(async);
    }

    public override Task Json_projection_enum_with_custom_conversion(bool async)
    {
        // this issue is with custom naming
        return base.Json_projection_enum_with_custom_conversion(async);
    }

    [Theory(Skip = EFCoreSkipReasons.BadMaterialization)]
    public override Task Project_json_reference_in_tracking_query_fails(bool async)
    {
        return base.Project_json_reference_in_tracking_query_fails(async);
    }
}
