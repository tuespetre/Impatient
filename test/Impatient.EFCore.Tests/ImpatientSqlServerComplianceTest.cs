using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Impatient.EFCore.Tests
{
    public class ImpatientSqlServerComplianceTest : RelationalComplianceTestBase
    {
        protected override Assembly TargetAssembly { get; } = typeof(ImpatientSqlServerComplianceTest).Assembly;

        protected override ICollection<Type> IgnoredTestBases { get; }
            = new List<Type>
            {
                // Raw SQL queries - not supported
                typeof(FromSqlQueryTestBase<>),
                typeof(FromSqlSprocQueryTestBase<>),
                typeof(GearsOfWarFromSqlQueryTestBase<>),
                typeof(NorthwindSqlQueryTestBase<>),
                typeof(SqlQueryTestBase<>),

                // EF specific warnings tests
                typeof(QueryNoClientEvalTestBase<>),
                typeof(WarningsTestBase<>),

                // Bulk updates - not our concern
                typeof(BulkUpdatesTestBase<>),
                typeof(ComplexTypeBulkUpdatesTestBase<>),
                typeof(FiltersInheritanceBulkUpdatesTestBase<>),
                typeof(InheritanceBulkUpdatesTestBase<>),
                typeof(NonSharedModelBulkUpdatesTestBase),
                typeof(NorthwindBulkUpdatesTestBase<>),
                typeof(TPCFiltersInheritanceBulkUpdatesTestBase<>),
                typeof(TPCInheritanceBulkUpdatesTestBase<>),
                typeof(TPHInheritanceBulkUpdatesTestBase<>),
                typeof(TPTFiltersInheritanceBulkUpdatesTestBase<>),
                typeof(TPTInheritanceBulkUpdatesTestBase<>),

                // Migrations - not our concern
                typeof(MigrationsInfrastructureTestBase<>),
                typeof(MigrationsTestBase<>),
                typeof(MigrationsSqlGeneratorTestBase),

                // Updates - not our concern
                typeof(JsonUpdateTestBase<>),
                typeof(NonSharedModelUpdatesTestBase),
                typeof(StoredProcedureUpdateTestBase),
                typeof(StoreValueGenerationTestBase<>),
                typeof(UpdateSqlGeneratorTestBase),

                // Miscellaneous updates - not our concern
                typeof(DataBindingTestBase<>),
                typeof(GraphUpdatesTestBase<>),
                typeof(MusicStoreImpatientTest),
                typeof(ProxyGraphUpdatesTestBase<>),
                typeof(TransactionTestBase<>),
                typeof(UpdatesRelationalTestBase<>),
                typeof(UpdatesTestBase<>),

                // The test is not built to use the TestStoreFactory, so sticking our compiler in is too tricky
                typeof(JsonTypesTestBase),
                typeof(JsonTypesRelationalTestBase),
                typeof(ModelBuilding101TestBase),
                typeof(ModelBuilding101RelationalTestBase),

                // Miscellaneous
                typeof(ApiConsistencyTestBase<>),
                typeof(DesignTimeTestBase<>),
                typeof(LoggingTestBase),
                typeof(LoggingRelationalTestBase<,>),
                typeof(SeedingTestBase),
            };
    }
}
