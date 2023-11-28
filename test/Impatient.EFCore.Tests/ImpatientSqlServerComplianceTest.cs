using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
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
                typeof(MigrationsTestBase<>),
                typeof(MigrationsSqlGeneratorTestBase),
            };
    }
}
