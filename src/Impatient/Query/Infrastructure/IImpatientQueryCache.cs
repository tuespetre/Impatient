using System;

namespace Impatient.Query.Infrastructure
{
    public interface IImpatientQueryCache
    {
        Delegate GetOrAdd<TArg>(int key, Func<TArg, Delegate> factory, TArg arg) where TArg : struct;
    }
}
