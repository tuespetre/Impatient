namespace Impatient.EFCore.Tests
{
    public static class EFCoreSkipReasons
    {
        public const string TestAssumesIntermediateNavigationsAreTracked =
            "The EF Core test assumes that an intermediate navigation " +
            "property is included in the results and the change tracker " +
            "but we do not include the intermediate property without an " +
            "explicit Include.";

        public const string TestAssumesNestedSubqueryResultsAreNotTracked =
            "The EF Core test assumes that a grouping's entries are not " +
            "immediately added to the change tracker, but we do immediately " +
            "track them under certain circumstances (like when using FOR JSON.)";

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
