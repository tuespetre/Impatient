using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Impatient.EFCore.Tests.Utilities
{
    public class ImpatientNorthwindTestStore : RelationalTestStore
    {
        public ImpatientNorthwindTestStore() : base("Northwind", true)
        {
            ConnectionString =
                $"Server=.\\sqlexpress; " +
                $"Database=impatient-efcore-northwind; " +
                $"Trusted_Connection=True";

            Connection = new SqlConnection(ConnectionString);
        }

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        {
            return builder.UseSqlServer(Connection);
        }

        public override void Clean(DbContext context)
        {
            new ImpatientDatabaseCleaner().Clean(context.Database);
        }

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
        {
            using (var context = createContext())
            {
                if (context.Database.EnsureCreated())
                {
                    var script 
                        = File.ReadAllText(
                            Path.Combine(
                                Path.GetDirectoryName(GetType().Assembly.Location), 
                                "Utilities/Northwind.sql"));

                    if (Connection.State != ConnectionState.Closed)
                    {
                        Connection.Close();
                    }

                    try
                    {
                        Connection.Open();

                        var batchRegex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);

                        foreach (var batch in batchRegex.Split(script).Where(b => !string.IsNullOrEmpty(b)))
                        {
                            using var command = Connection.CreateCommand();
                            command.CommandText = batch;
                            command.ExecuteNonQuery();
                        }
                    }
                    finally
                    {
                        if (Connection.State != ConnectionState.Closed)
                        {
                            Connection.Close();
                        }
                    }
                }
            }
        }

        private void ExecuteScript(string scriptPath)
        {
        }
    }
}
