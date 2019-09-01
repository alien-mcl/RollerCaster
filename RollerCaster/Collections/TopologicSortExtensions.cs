using System;
using System.Collections.Generic;
using System.Linq;

namespace RollerCaster.Collections
{
    /// <summary>Extension method to implement topologic sorting of <see cref="Type" />s.
    /// Will return a list where types with specializations will come after items without specializations.
    /// </summary>
    /// <remarks>From http://stackoverflow.com/questions/21189222/topological-sort-with-support-for-cyclic-dependencies .</remarks>
    internal static class TopologicSortExtensions
    {
        /// <summary>Sorts a list of types so those without specializations come first.</summary>
        /// <param name="types">List of types to sort.</param>
        /// <returns>A sorted list of item, where items with dependencies after items without dependencies.</returns>
        internal static IList<Type> TopologicSort(this IList<Type> types)
        {
            int[] state = new int[types.Count];
            var result = new List<Type>();

            for (int index = 0; index < types.Count; index++)
            {
                state[index] = 1;
            }

            for (int index = 0; index < types.Count; index++)
            {
                Visit(types, index, result, state);
            }

            return result;
        }

        private static void Visit(IList<Type> types, int index, IList<Type> result, int[] state)
        {
            if (state[index] == -1 || state[index] == 0)
            {
                return;
            }

            state[index] = 0;
            foreach (var neighbour in GetNeighbours(types, types[index]))
            {
                Visit(types, types.IndexOf(neighbour), result, state);
            }

            state[index] = -1;
            result.Add(types[index]);
        }

        private static IList<Type> GetNeighbours(IList<Type> types, Type node)
        {
            var neighbours = types.Where(type => type.DependsOn(node)).ToList();
            var unique = new List<Type>();
            neighbours.ForEach(type =>
            {
                if (unique.All(other => other != type))
                {
                    unique.Add(type);
                }
            });

            if (unique.IndexOf(node) > -1)
            {
                unique.Remove(node);
            }

            return unique;
        }

        private static bool DependsOn(this Type type, Type another)
        {
            if (another.IsAssignableFrom(type))
            {
                return true;
            }

            int typeAssemblyHashCode = type.Assembly.GetHashCode();
            int anotherAssemblyHashCode = another.Assembly.GetHashCode();
            if (typeAssemblyHashCode != anotherAssemblyHashCode)
            {
                return (type.Assembly.GetReferencedAssemblies().Contains(another.Assembly.GetName()));
            }

            return false;
        }
    }
}