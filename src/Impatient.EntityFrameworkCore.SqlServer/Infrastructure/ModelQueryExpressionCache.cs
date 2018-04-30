using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public class ModelQueryExpressionCache
    {
        public Dictionary<IntPtr, Expression> Lookup { get; } = new Dictionary<IntPtr, Expression>();
    }
}
