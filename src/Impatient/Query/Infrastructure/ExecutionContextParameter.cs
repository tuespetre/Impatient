using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public static class ExecutionContextParameter
    {
        public static ParameterExpression Instance { get; } = Expression.Parameter(typeof(IDbCommandExecutor), "executor");
    }
}
