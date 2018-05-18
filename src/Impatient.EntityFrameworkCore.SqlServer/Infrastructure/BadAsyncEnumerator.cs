using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal class BadAsyncEnumerator<TResult> : IAsyncEnumerator<TResult>
    {
        private readonly Task<IEnumerator<TResult>> task;
        private IEnumerator<TResult> enumerator;

        public BadAsyncEnumerator(Task<IEnumerator<TResult>> task)
        {
            this.task = task;
        }

        public TResult Current => enumerator.Current;

        public void Dispose()
        {
            task.Dispose();
            enumerator?.Dispose();
        }

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            if (enumerator == null)
            {
                enumerator = await task;
            }

            return enumerator.MoveNext();
        }
    }
}
