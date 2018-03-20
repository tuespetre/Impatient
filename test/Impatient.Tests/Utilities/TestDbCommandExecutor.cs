using Impatient.Query;
using Impatient.Query.Infrastructure;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Impatient.Tests.Utilities
{
    public class TestDbCommandExecutor : DefaultDbCommandExecutor
    {
        private const string DefaultConnectionString = @"Server=.\sqlexpress; Database=Impatient; Trusted_Connection=True";

        private readonly string connectionString;

        public StringBuilder Log { get; } = new StringBuilder();

        public TestDbCommandExecutor(string connectionString = null)
        {
            this.connectionString = connectionString ?? DefaultConnectionString;
        }

        protected override DbConnection GetDbConnection()
        {
            return new SqlConnection(connectionString);
        }

        protected override void OnCommandInitialized(DbCommand command)
        {
            if (Log.Length > 0)
            {
                Log.AppendLine().AppendLine();
            }

            Log.Append(command.CommandText);
        }
    }
}
