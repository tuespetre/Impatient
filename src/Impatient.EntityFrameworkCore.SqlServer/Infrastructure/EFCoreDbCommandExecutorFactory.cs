using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure;

public class EFCoreDbCommandExecutorFactory : IDbCommandExecutorFactory
{
    private readonly ICurrentDbContext currentDbContext;
    private readonly IRelationalCommandDiagnosticsLogger logger;

    public EFCoreDbCommandExecutorFactory(
        ICurrentDbContext currentDbContext,
        IRelationalCommandDiagnosticsLogger logger)
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
