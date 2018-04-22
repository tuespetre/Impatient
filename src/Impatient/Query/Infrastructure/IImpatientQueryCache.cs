using System;

namespace Impatient.Query.Infrastructure
{
    public interface IImpatientQueryCache
    {
        bool TryGetValue(int key, out Delegate value);

        void Add(int key, Delegate value);
    }
}
