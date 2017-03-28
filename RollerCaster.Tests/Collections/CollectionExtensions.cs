using System.Collections.Generic;

namespace RollerCaster.Collections
{
    internal static class CollectionExtensions
    {
        internal static ICollection<T> AddNext<T>(this ICollection<T> collection, T item)
        {
            collection.Add(item);
            return collection;
        }
    }
}
