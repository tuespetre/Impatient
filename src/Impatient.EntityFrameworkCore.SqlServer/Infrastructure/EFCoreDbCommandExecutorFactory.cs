using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public class EFCoreDbCommandExecutorFactory : IDbCommandExecutorFactory
    {
        private readonly ICurrentDbContext currentDbContext;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger;

        public EFCoreDbCommandExecutorFactory(
            ICurrentDbContext currentDbContext,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger)
        {
            this.currentDbContext = currentDbContext;
            this.logger = logger;
        }

        public IDbCommandExecutor Create()
        {
            return new EFCoreDbCommandExecutor(
                currentDbContext,
                logger);
        }
    }
}
