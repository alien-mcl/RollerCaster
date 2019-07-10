using System;
using System.Collections.Generic;

namespace RollerCaster.Collections
{
    internal static class DictionaryExtensions
    {
        internal static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out bool success)
        {
            return dictionary.TryGet(key, null, out success);
        }

        internal static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValue)
        {
            bool success;
            return dictionary.TryGet(key, defaultValue, out success);
        }

        private static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValue, out bool success)
        {
            success = true;
            TValue result;
            if (dictionary == null || !dictionary.TryGetValue(key, out result))
            {
                if (defaultValue != null)
                {
                    result = defaultValue();
                    dictionary[key] = result;
                }
                else
                {
                    result = default(TValue);
                }

                success = false;
            }

            return result;
        }
    }
}
