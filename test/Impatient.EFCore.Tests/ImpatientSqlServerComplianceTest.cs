using Microsoft.EntityFrameworkCore;
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
                typeof(MigrationSqlGeneratorTestBase),
                typeof(MigrationsTestBase<>),
                typeof(AsyncFromSqlQueryTestBase<>),
                typeof(AsyncFromSqlSprocQueryTestBase<>),
                typeof(FromSqlQueryTestBase<>),
                typeof(FromSqlSprocQueryTestBase<>),
                typeof(GearsOfWarFromSqlQueryTestBase<>),
                typeof(QueryNoClientEvalTestBase<>),
                typeof(WarningsTestBase<>),
            };
    }
}
