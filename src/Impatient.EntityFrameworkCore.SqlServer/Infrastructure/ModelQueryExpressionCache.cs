using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure;

public class ModelQueryExpressionCache
{
    public ConcurrentDictionary<IEntityType, Expression> Lookup { get; } 
        = new ConcurrentDictionary<IEntityType, Expression>();
}
