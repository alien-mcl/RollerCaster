using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using RollerCaster.Collections;

namespace RollerCaster.Reflection
{
    /// <summary>Defines collection creation options.</summary>
    [Flags]
    public enum CollectionOptions
    {
        /// <summary>Defines no additional collection options.</summary>
        None = 0,

        /// <summary>Defines an observable collection option.</summary>
        Observable = 1,

        /// <summary>Defines a concurrent collection option.</summary>
        Concurrent = 2
    }

    /// <summary>Provides useful <see cref="Type" /> extension methods.</summary>
    public static class TypeExtensions
    {
        private static readonly Type StringType = typeof(string);
        private static readonly Type ArrayOfBytesType = typeof(byte[]);

        private static readonly IDictionary<Type, IDictionary<CollectionOptions, Type>> CollectionConstructors =
            new Dictionary<Type, IDictionary<CollectionOptions, Type>>()
            {
                {
                    typeof(ISet<>),
                    new Dictionary<CollectionOptions, Type>()
                        {
                            { CollectionOptions.None, typeof(HashSet<>) },
                            { CollectionOptions.Observable, typeof(HashSet<>) }
                        }
                },
                {
                    typeof(IList<>),
                    new Dictionary<CollectionOptions, Type>()
                        {
                            { CollectionOptions.None, typeof(List<>) },
                            { CollectionOptions.Observable, typeof(ObservableList<>) },
                            { CollectionOptions.Concurrent, typeof(ObservableList<>) },
                            { CollectionOptions.Observable | CollectionOptions.Concurrent, typeof(ObservableList<>) }
                        }
                },
                {
                    typeof(IDictionary),
                    new Dictionary<CollectionOptions, Type>()
                        {
                            { CollectionOptions.None, typeof(Hashtable) },
                            { CollectionOptions.Observable, typeof(Hashtable) }
                        }
                },
                {
                    typeof(IDictionary<,>),
                    new Dictionary<CollectionOptions, Type>()
                        {
                            { CollectionOptions.None, typeof(Dictionary<,>) },
                            { CollectionOptions.Concurrent, typeof(ConcurrentDictionary<,>) },
                            { CollectionOptions.Observable, typeof(ConcurrentDictionary<,>) }
                        }
                }
            };

        /// <summary>Determines whether the given <paramref name="type" /> is an enumerable type.</summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns><b>true</b> if the <paramref name="type" /> is enumerable, but not neither a string nor an array of bytes; otherwise, <b>false</b>.</returns>
        public static bool IsAnEnumerable(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return (typeof(IEnumerable).IsAssignableFrom(type)) && (type != StringType) && (type != ArrayOfBytesType);
        }

        /// <summary>Determines whether the given <paramref name="type" /> is a list type.</summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns><b>true</b> if the <paramref name="type" /> is a list, but not an array; otherwise, <b>false</b>.</returns>
        public static bool IsAList(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsArray)
            {
                return false;
            }

            var typeInfo = type.GetTypeInfo();
            return (typeof(IList).IsAssignableFrom(type)) || (typeof(IList<>).IsAssignableFrom(type)) || 
                ((typeInfo.IsGenericType) && typeof(IList<>).IsAssignableFrom(type.GetGenericTypeDefinition())) ||
                (type.GetInterfaces().Any(@interface => @interface.GetTypeInfo().IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IList<>)));
        }

        /// <summary>Determines whether the given <paramref name="type" /> is a set.</summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns><b>true</b> if the <paramref name="type" /> is a set; otherwise, <b>false</b>.</returns>
        public static bool IsASet(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.GetTypeInfo().IsGenericType)
            {
                return false;
            }
            
            var genericType = type.GetGenericTypeDefinition();
            return (type == typeof(ISet<>)) || (typeof(ISet<>).IsAssignableFrom(type)) || (typeof(ISet<>).IsAssignableFrom(genericType)) ||
                (genericType.GetInterfaces().Any(@interface => @interface.GetTypeInfo().IsGenericType && @interface.GetGenericTypeDefinition() == typeof(ISet<>)));
        }

        /// <summary>Determines whether the given <paramref name="type" /> is a dictionary.</summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns><b>true</b> if the <paramref name="type" /> is a dictionary; otherwise, <b>false</b>.</returns>
        public static bool IsADictionary(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if ((typeof(IDictionary) == type) || (typeof(IDictionary).IsAssignableFrom(type)))
            {
                return true;
            }

            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return false;
            }
            
            var genericType = type.GetGenericTypeDefinition();
            return (type == typeof(IDictionary<,>)) || (genericType == typeof(IDictionary<,>)) || (typeof(IDictionary<,>).IsAssignableFrom(genericType));
        }

        /// <summary>Creates a default value for <paramref name="valueType" />.</summary>
        /// <remarks>
        /// This method uses <see cref="Activator" /> for creating value types. 
        /// If the type is enumerable except <see cref="String" /> and <see cref="byte" />[] it will create 
        /// a proper <see cref="IList{T}" />, <see cref="IList{T}"/> and <see cref="IDictionary{TKey,TValue}" /> respectively.
        /// </remarks>
        /// <param name="valueType">Type of the value.</param>
        /// <param name="options">Allows to define collection special capabilities.</param>
        /// <returns>Instance of the <paramref name="valueType" />.</returns>
        public static object GetDefaultValue(this Type valueType, CollectionOptions options = CollectionOptions.Observable)
        {
            if (valueType == null)
            {
                return null;
            }

            var valueTypeInfo = valueType.GetTypeInfo();
            if (!valueType.IsAnEnumerable())
            {
                return (valueTypeInfo.IsValueType ? Activator.CreateInstance(valueType) : null);
            }

            Type collectionType;
            if (valueType.IsADictionary())
            {
                collectionType = (!valueTypeInfo.IsGenericType
                    ? CollectionConstructors[typeof(IDictionary)][options]
                    : CollectionConstructors[typeof(IDictionary<,>)][options].MakeGenericType(valueType.GetGenericArguments()));
            }
            else if (valueType.IsASet())
            {
                collectionType = CollectionConstructors[typeof(ISet<>)][options].MakeGenericType(valueType.GetItemType());
            }
            else
            {
                collectionType = CollectionConstructors[typeof(IList<>)][options].MakeGenericType(valueType.GetItemType());
            }

            return collectionType.GetConstructor(Type.EmptyTypes).Invoke(null);
        }

        /// <summary>Gets a type of an item in the collection.</summary>
        /// <remarks>If the given <paramref name="type" /> is not a collection, same <paramref name="type" /> will be returned.</remarks>
        /// <param name="type">Type to be probed.</param>
        /// <returns>Item type of the collection.</returns>
        public static Type GetItemType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsArray)
            {
                return type.GetElementType();
            }

            var typeInfo = type.GetTypeInfo();
            return (typeInfo.IsGenericType ? type.GetGenericArguments()[0] : type);
        }

        internal static void AddEnumerationValue(this Type valueType, IEnumerable currentValue, object value)
        {
            var add = currentValue.GetType().GetMethod("Add");
            currentValue.Add(add, valueType.IsADictionary(), value);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", MessageId = "System.String.EndsWith(System.String)", Justification = "String is culture invariant.")]
        internal static void CopyEnumeration(this Type valueType, IEnumerable currentValue, IEnumerable value)
        {
            var currentValueTypeInfo = currentValue.GetType();
            var add = currentValueTypeInfo.GetMethod("Add");
            if (add == null)
            {
                add = (from method in valueType.GetRuntimeMethods()
                       where method.Name == "Add" || method.Name.EndsWith(".Add")
                       from parameter in method.GetParameters()
                       join genericType in currentValueTypeInfo.GenericTypeArguments on parameter.ParameterType equals genericType into parameters
                       select method).First();
            }

            bool isDictionary = valueType.IsADictionary();
            foreach (var item in value)
            {
                currentValue.Add(add, isDictionary, item);
            }
        }

        private static void Add(this IEnumerable currentValue, MethodInfo add, bool isDictionary, object value)
        {
            var parameters = new List<object>(2);
            if (isDictionary)
            {
                var itemTypeInfo = value.GetType();
                parameters.Add(itemTypeInfo.GetProperty("Key").GetValue(value));
                parameters.Add(itemTypeInfo.GetProperty("Value").GetValue(value));
            }
            else
            {
                parameters.Add(value);
            }

            add.Invoke(currentValue, parameters.ToArray());
        }
    }
}
