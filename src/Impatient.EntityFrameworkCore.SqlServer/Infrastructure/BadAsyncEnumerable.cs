using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal class BadAsyncEnumerable<TResult> : IAsyncEnumerable<TResult>
    {
        private readonly Func<Task<IEnumerator<TResult>>> func;

        public BadAsyncEnumerable(Func<Task<IEnumerator<TResult>>> func)
        {
            this.func = func;
        }

        public IAsyncEnumerator<TResult> GetEnumerator()
        {
            return new BadAsyncEnumerator<TResult>(func());
        }
    }
}
