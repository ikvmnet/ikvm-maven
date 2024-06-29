using System.Collections.Generic;

namespace IKVM.Maven.Sdk.Tasks.Extensions
{
    static class DictionaryExtensions
    {

#if NETFRAMEWORK

        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue value)
        {
            if (self.ContainsKey(key) == false)
            {
                self.Add(key, value);
                return true;
            }
            else
                return false;
        }

#endif

    }
}
