using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class SimpleQueryImpatientTest : SimpleQueryRelationalTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        [ConditionalTheory(Skip = EFCoreSkipReasons.FromSql)]
        [MemberData(nameof(IsAsyncData))]
        public override Task Multiple_different_entity_type_from_different_namespaces(bool async)
        {
            return base.Multiple_different_entity_type_from_different_namespaces(async);
        }
    }
}
