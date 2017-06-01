using Impatient.Query;
using System.Data.Common;
using System.Data.SqlClient;

namespace Impatient.Tests.Utilities
{
    public class TestImpatientConnectionFactory : IImpatientDbConnectionFactory
    {
        private readonly string connectionString;

        public TestImpatientConnectionFactory()
        {
            connectionString = @"Server=.\sqlexpress; Database=Impatient; Trusted_Connection=True";
        }

        public TestImpatientConnectionFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
