using System;
using System.Collections.Generic;

namespace Impatient.Query.Infrastructure
{
    public class DefaultImpatientQueryCache : IImpatientQueryCache
    {
        private readonly Dictionary<int, Delegate> dictionary
            = new Dictionary<int, Delegate>();

        public Delegate GetOrAdd<TArg>(int key, Func<TArg, Delegate> factory, TArg arg) where TArg : struct
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            lock (dictionary)
            {
                if (!dictionary.TryGetValue(key, out var value))
                {
                    dictionary.Add(key, value = factory(arg));
                }

                return value;
            }
        }
    }
}
