using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public class ModelQueryExpressionCache
    {
        public ConcurrentDictionary<IntPtr, Expression> Lookup { get; } 
            = new ConcurrentDictionary<IntPtr, Expression>();
    }
}
