using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public static class DbContextParameter
    {
        private static ConcurrentDictionary<Type, ParameterExpression> instances 
            = new ConcurrentDictionary<Type, ParameterExpression>();

        public static ParameterExpression GetInstance(Type type)
        {
            return instances.GetOrAdd(type, t => Expression.Parameter(type, "dbContext"));
        }
    }
}
