using Microsoft.EntityFrameworkCore.Query;
using System.Data.Common;
using System.Data.SqlClient;

namespace Impatient.EFCore.Tests.Query
{
    public class SqlExecutorImpatientTest : SqlExecutorTestBase<NorthwindQueryImpatientFixture>
    {
        public SqlExecutorImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        protected override string TenMostExpensiveProductsSproc => "[dbo].[Ten Most Expensive Products]";

        protected override string CustomerOrderHistorySproc => "[dbo].[CustOrderHist] @CustomerID";

        protected override string CustomerOrderHistoryWithGeneratedParameterSproc => "[dbo].[CustOrderHist] @CustomerID = {0}";

        protected override DbParameter CreateDbParameter(string name, object value)
        {
            return new SqlParameter
            {
                ParameterName = name,
                Value = value
            };
        }
    }
}
