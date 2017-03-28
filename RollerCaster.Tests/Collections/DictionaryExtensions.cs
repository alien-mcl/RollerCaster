using System.Collections.Generic;

namespace RollerCaster.Collections
{
    internal static class DictionaryExtensions
    {
        internal static IDictionary<TKey, TValue> AddNext<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            dictionary.Add(key, value);
            return dictionary;
        }
    }
}
