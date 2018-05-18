using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class ImpatientSqlServerComplianceTest : ComplianceTestBase
    {
        protected override Assembly TargetAssembly { get; } = typeof(ImpatientSqlServerComplianceTest).Assembly;

        protected override ICollection<Type> IgnoredTestBases { get; } = new List<Type>();
    }
}
