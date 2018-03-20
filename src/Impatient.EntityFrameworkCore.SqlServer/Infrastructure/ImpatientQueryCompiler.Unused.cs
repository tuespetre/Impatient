using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public partial class ImpatientQueryCompiler
    {
        // These don't seem like they should be part of the IQueryCompiler interface.

        public Func<QueryContext, IAsyncEnumerable<TResult>> CreateCompiledAsyncEnumerableQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        public Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }

        public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            throw new NotImplementedException();
        }
    }
}
