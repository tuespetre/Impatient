using System;

namespace Impatient.Query
{
    public interface IImpatientQueryCache
    {
        bool TryGetValue(int key, out Delegate value);

        void Add(int key, Delegate value);
    }
}
