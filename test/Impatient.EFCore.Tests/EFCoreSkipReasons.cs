namespace Impatient.EFCore.Tests;

public static class EFCoreSkipReasons
{
    public const string Punt =
        "This test should be fixed and unskipped at some point but it covers " +
        "functionality that is not critical.";

    public const string ManualLeftJoinNullabilityPropagation =
        "This test relies on the detection of the 'manual left join' pattern during " +
        "navigation composition, which is not yet supported.";

    public const string NullNavigationProtection =
        "This test requires or demonstrates the concept of null navigation property " +
        "access protection during client evaluation. We may or may not want to support that.";

    public const string FromSql =
        "FromSql is not supported.";

    public const string SpecialIncludes =
        "Not currently supporting filtered or ordered includes.";

    public const string TestRunAborts =
        "The test is currently aborting when XUnit runs it. Should be fixed.";

    public const string TranslationBeyondEF =
        "The test demonstrates a query that Impatient translates but EF does not.";

    public const string ClientEval =
        "The test should 'pass' by failing with an InvalidOperationException for client evaluation to comply with EF Core expectations.";

    public const string BadMaterialization =
        "The test should 'pass' by failing with an InvalidOperationException for a 'bad' materializer to comply with EF Core expectations.";

    public const string SkipNavigations =
        "Have not yet added support for skip navigations.";

    public const string LiftedInclude =
        "This test relies on a 'lifted include', which has not yet been implemented.";

    public const string OrderByEntity =
        "This test relies on ordering by an entity being translated to ordering by its primary key, which is not yet implemented.";
}
