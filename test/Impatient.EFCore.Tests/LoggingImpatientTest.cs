using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using System;

namespace Impatient.EFCore.Tests
{
    // *shrug*
    public class LoggingImpatientTest : LoggingRelationalTestBase<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>
    {
        protected override string ProviderName => "Microsoft.EntityFrameworkCore.SqlServer";

        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            Action<RelationalDbContextOptionsBuilder<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder().UseSqlServer("Data Source=LoggingSqlServerTest.db", relationalAction);
    }
}
