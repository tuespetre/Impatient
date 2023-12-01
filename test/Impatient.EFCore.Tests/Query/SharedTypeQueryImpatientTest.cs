using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class SharedTypeQueryImpatientTest : SharedTypeQueryRelationalTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void Ad_hoc_query_for_shared_type_entity_type_works()
        {
            base.Ad_hoc_query_for_shared_type_entity_type_works();
        }

        [ConditionalTheory(Skip = EFCoreSkipReasons.FromSql)]
        [MemberData(nameof(IsAsyncData))]
        public override Task Can_use_shared_type_entity_type_in_query_filter_with_from_sql(bool async)
        {
            return base.Can_use_shared_type_entity_type_in_query_filter_with_from_sql(async);
        }
    }
}
