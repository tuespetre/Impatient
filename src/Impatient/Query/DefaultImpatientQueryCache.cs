using System;
using System.Collections.Generic;

namespace Impatient.Query
{
    public class DefaultImpatientQueryCache : IImpatientQueryCache
    {
        private readonly Dictionary<int, Delegate> dictionaryCache = new Dictionary<int, Delegate>();

        public void Add(int key, Delegate value) => dictionaryCache.Add(key, value);

        public bool TryGetValue(int key, out Delegate value) => dictionaryCache.TryGetValue(key, out value);
    }
}
