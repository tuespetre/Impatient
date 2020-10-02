using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

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
            // had to add this try/catch block since updating efcore refs to 5.x,
            // looks like they handle this in latest source tho, packages not up to date yet?
            try
            {
                context.Database.EnsureCreated();
            }
            catch (SqlException sql) when (sql.Number is 4060)
            {
                var creator = context.GetService<IRelationalDatabaseCreator>();

                creator.Create();
                creator.CreateTables();
            }

            new ImpatientDatabaseCleaner().Clean(context.Database);
        }
    }
}
