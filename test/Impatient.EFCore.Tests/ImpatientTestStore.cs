using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data.SqlClient;

namespace Impatient.EFCore.Tests
{
    public class ImpatientTestStore : RelationalTestStore
    {
        private DbTransaction transaction;

        public ImpatientTestStore(string connectionString = null)
        {
            if (connectionString != null)
            {
                Connection = new SqlConnection(connectionString)
                {
                    ConnectionString = connectionString
                };

                ConnectionString = connectionString;
            }
        }

        public override DbConnection Connection { get; }

        public override DbTransaction Transaction => transaction;

        public override string ConnectionString { get; }

        public override void OpenConnection()
        {
            if (Connection != null)
            {
                Connection.Open();
                transaction = Connection.BeginTransaction();
            }
        }
    }
}
