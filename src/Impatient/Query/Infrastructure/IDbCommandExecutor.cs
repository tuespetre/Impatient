using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Impatient.Query.Infrastructure
{
    public interface IDbCommandExecutor
    {
        IEnumerable<TElement> ExecuteEnumerable<TElement>(Action<DbCommand> initializer, Func<DbDataReader, TElement> materializer);

        TResult ExecuteComplex<TResult>(Action<DbCommand> initializer, Func<DbDataReader, TResult> materializer);

        TResult ExecuteScalar<TResult>(Action<DbCommand> initializer);
    }
}
