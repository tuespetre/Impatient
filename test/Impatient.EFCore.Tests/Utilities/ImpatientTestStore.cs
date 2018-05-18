using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Data.SqlClient;

namespace Impatient.EFCore.Tests.Utilities
{
    public class ImpatientTestStore : RelationalTestStore
    {
        public ImpatientTestStore(string name, bool shared) : base(name, shared)
        {
            ConnectionString = 
                $"Server=.\\sqlexpress; " +
                $"Database=impatient-efcore-{name.ToLowerInvariant()}; " +
                $"Trusted_Connection=True";

            Connection = new SqlConnection(ConnectionString)
            {
                ConnectionString = ConnectionString
            };
        }

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        {
            return builder.UseSqlServer(Connection);
        }

        public override void Clean(DbContext context)
        {
            new ImpatientDatabaseCleaner().Clean(context.Database);
        }
    }
}
