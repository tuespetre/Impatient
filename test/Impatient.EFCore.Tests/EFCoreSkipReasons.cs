namespace Impatient.EFCore.Tests
{
    public static class EFCoreSkipReasons
    {
        public const string TestAssumesIntermediateNavigationsAreTracked =
            "The EF Core test assumes that an intermediate navigation " +
            "property is included in the results and the change tracker " +
            "but we do not include the intermediate property without an " +
            "explicit Include.";

        public const string TestAssumesGroupedEntitiesAreNotTracked =
            "The EF Core test assumes that a grouping's entries are not " +
            "immediately added to the change tracker, but we do immediately " +
            "track them under certain circumstances (like when using FOR JSON.)";

        public const string TestIncorrectlyAssumesReturnedEntitiesAreNotTracked =
            "The EF Core test assumes that an entity returned in the results " +
            "is not tracked but this is semantically incorrect.";

        public const string PessimisticTracking =
            "EF Core assumes that entities should not be tracked when passing " +
            "through a client selector (optimistic) but we cannot guarantee that " +
            "the selector will not cause the entities to be returned so we do track " +
            "them (pessimistic.)";

        public const string Punt =
            "This test should be fixed and unskipped at some point but it covers " +
            "functionality that is not critical.";

        public const string TestReliesOnUnguaranteedOrder =
            "This test relies on certain assumptions about the order of results that " +
            "is not guaranteed by the semantics of the query.";
    }
}
