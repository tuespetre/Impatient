using System.Linq.Expressions;
using System.Threading;

namespace Impatient.Query.Infrastructure
{
    public static class ExecutionContextParameters
    {
        public static ParameterExpression DbCommandExecutor { get; } = Expression.Parameter(typeof(IDbCommandExecutor), "executor");

        public static ParameterExpression CancellationToken { get; } = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
    }
}
