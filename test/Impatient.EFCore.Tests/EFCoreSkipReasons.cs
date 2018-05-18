namespace Impatient.EFCore.Tests
{
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

        public const string CompiledQueries =
            "Compiled queries are not yet supported.";

        public const string FromSql =
            "FromSql is not supported.";
    }
}
