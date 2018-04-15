namespace Impatient.Query.Infrastructure
{
    public interface IDbCommandExecutorFactory
    {
        IDbCommandExecutor Create();
    }
}
