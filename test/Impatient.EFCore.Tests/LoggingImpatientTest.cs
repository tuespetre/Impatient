using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Impatient.EFCore.Tests
{
    // *shrug*
    public class LoggingImpatientTest : LoggingRelationalTestBase<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>
    {
        protected override string ProviderName => "Microsoft.EntityFrameworkCore.SqlServer";

        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            IServiceCollection services, 
            Action<RelationalDbContextOptionsBuilder<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>> relationalAction)
        {
            return new DbContextOptionsBuilder().UseSqlServer("Data Source=LoggingSqlServerTest.db", relationalAction);
        }
    }
}
