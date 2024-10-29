using Impatient.Query.Infrastructure;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Text;

namespace Impatient.Tests.Utilities
{
    public class TestDbCommandExecutorFactory : IDbCommandExecutorFactory
    {
        private readonly string connectionString;

        private TestDbCommandExecutor instance;

        public TestDbCommandExecutorFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IDbCommandExecutor Create()
        {
            return instance ?? (instance = new TestDbCommandExecutor(connectionString));
        }

        public StringBuilder Log => instance?.Log;
    }

    public class TestDbCommandExecutor : BaseDbCommandExecutor
    {
        private const string DefaultConnectionString = @"Server=.\sqlexpress; Database=Impatient; Trusted_Connection=True; TrustServerCertificate=true";

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
