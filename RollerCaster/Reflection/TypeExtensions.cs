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

        private static readonly IDictionary<CollectionOptions, Type> ListConstructors = new Dictionary<CollectionOptions, Type>()
        {
            { CollectionOptions.None, typeof(List<>) },
            { CollectionOptions.Observable, typeof(ObservableList<>) },
            { CollectionOptions.Concurrent, typeof(ObservableList<>) },
            { CollectionOptions.Observable | CollectionOptions.Concurrent, typeof(ObservableList<>) }
        };

        private static readonly IDictionary<CollectionOptions, Type> SetConstructors = new Dictionary<CollectionOptions, Type>()
        {
            { CollectionOptions.None, typeof(HashSet<>) },
            { CollectionOptions.Observable, typeof(ObservableSet<>) },
            { CollectionOptions.Concurrent, typeof(ObservableSet<>) },
            { CollectionOptions.Observable | CollectionOptions.Concurrent, typeof(ObservableSet<>) }
        };

        private static readonly IDictionary<CollectionOptions, Type> DictionaryConstructors = new Dictionary<CollectionOptions, Type>()
        {
            { CollectionOptions.None, typeof(Dictionary<,>) },
            { CollectionOptions.Observable, typeof(ObservableDictionary<,>) },
            { CollectionOptions.Concurrent, typeof(ObservableDictionary<,>) },
            { CollectionOptions.Observable | CollectionOptions.Concurrent, typeof(ObservableDictionary<,>) }
        };

        private static readonly IDictionary<Type, IDictionary<CollectionOptions, Type>> CollectionConstructors =
            new Dictionary<Type, IDictionary<CollectionOptions, Type>>()
            {
                { typeof(IEnumerable<>), ListConstructors },
                { typeof(ICollection<>), ListConstructors },
                { typeof(IList<>), ListConstructors },
                { typeof(ISet<>), SetConstructors },
                { typeof(IDictionary<,>), DictionaryConstructors },
                { typeof(IEnumerable), ListConstructors },
                { typeof(ICollection), ListConstructors },
                { typeof(IList), ListConstructors },
                { typeof(IDictionary), DictionaryConstructors }
            };

        private static readonly Type[] GenericCollections =
        {
            typeof(IEnumerable<>),
            typeof(ICollection<>),
            typeof(IList<>),
            typeof(IDictionary<,>),
            typeof(ISet<>)
        };

        private static readonly Type[] NonGenericCollections =
        {
            typeof(IEnumerable),
            typeof(ICollection),
            typeof(IList),
            typeof(IDictionary)
        };

        private static readonly Type[] NonGenericCollectionTypes =
        {
            typeof(IEnumerable),
            typeof(ICollection),
            typeof(IList),
            typeof(IDictionary)
        };

        private static readonly Type[] GenericCollectionTypes =
        {
            typeof(IEnumerable<>),
            typeof(ICollection<>),
            typeof(IList<>),
            typeof(IDictionary<,>),
            typeof(ISet<>)
        };

        private static readonly IDictionary<Type, bool> ReadOnlyCollections = new Dictionary<Type, bool>();

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

            return (typeof(IList).IsAssignableFrom(type)) || (typeof(IList<>).IsAssignableFrom(type)) || 
                ((type.IsGenericType) && typeof(IList<>).IsAssignableFrom(type.GetGenericTypeDefinition())) ||
                (type.GetInterfaces().Any(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IList<>)));
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

            if (!type.IsGenericType)
            {
                return false;
            }
            
            var genericType = type.GetGenericTypeDefinition();
            return (type == typeof(ISet<>)) || (typeof(ISet<>).IsAssignableFrom(type)) || (typeof(ISet<>).IsAssignableFrom(genericType)) ||
                (genericType.GetInterfaces().Any(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(ISet<>)));
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

            if (!type.IsGenericType)
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

            if (!valueType.IsAnEnumerable())
            {
                return valueType.CreateDefaultLiteral();
            }

            if (valueType.IsArray)
            {
                return Array.CreateInstance(valueType.GetElementType(), 0);
            }

            object[] parameters = null;
            ConstructorInfo ctor;
            IDictionary<CollectionOptions, Type> ctors;
            var collectionType = valueType.GetBaseCollectionType();
            if (collectionType != null && CollectionConstructors.TryGetValue(collectionType, out ctors))
            {
                var type = ctors[options];
                ctor = (collectionType.IsGenericType
                        ? type.MakeGenericType(valueType.GetGenericArguments())
                        : type.MakeGenericType(Enumerable.Range(0, type.GenericTypeArguments.Length).Select(_ => typeof(object)).ToArray()))
                    .GetConstructor(Type.EmptyTypes);
            }
            else
            {
                ctor = (
                    from method in valueType.GetConstructors()
                    where method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType.IsAnEnumerable()
                    select method).FirstOrDefault();
                if (ctor != null)
                {
                    parameters = new[]
                    {
                        !valueType.IsAssignableFrom(ctor.GetParameters()[0].ParameterType)
                            ? ctor.GetParameters()[0].ParameterType.GetDefaultValue(CollectionOptions.None)
                            : valueType.CreateDefaultLiteral()
                    };
                }
                else
                {
                    ctor = valueType.GetConstructor(Type.EmptyTypes);
                }
            }

            return ctor.Invoke(parameters);
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

            if (!type.IsGenericType)
            {
                return type;
            }

            var genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition.GetInterfaces().Any(@interface => @interface == typeof(IDictionary<,>)))
            {
                return typeof(KeyValuePair<,>).MakeGenericType(type.GetGenericArguments());
            }

            if (genericTypeDefinition.GetInterfaces().Any(@interface => @interface == typeof(IDictionary)))
            {
                return typeof(DictionaryEntry);
            }

            return type.GetGenericArguments()[0];
        }

        /// <summary>Checks whether the given <paramref name="type" /> can have a <i>null</i> value.</summary>
        /// <param name="type">Type to check.</param>
        /// <returns><i>true</i> in case a given <paramref name="type" /> is either a class or <see cref="Nullable{T}" />; otherwise <i>false</i>.</returns>
        public static bool CanHaveNullValue(this Type type)
        {
            return type != null && (type.IsClass || (type.IsValueType && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>Finds a property in the given <paramref name="type" /> including it's implemented interfaces.</summary>
        /// <param name="type">Type to search through.</param>
        /// <param name="name">Property name to search for.</param>
        /// <param name="propertyType">Optional property type.</param>
        /// <returns>Property matching a given <paramref name="name" /> or <b>null</b>.</returns>
        public static PropertyInfo FindProperty(this Type type, string name, Type propertyType = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (name.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(name));
            }

            var result = (propertyType != null ? type.GetProperty(name, propertyType) : type.GetProperty(name));
            if (result == null)
            {
                foreach (var @interface in type.GetInterfaces())
                {
                    result = (propertyType != null ? @interface.GetProperty(name, propertyType) : @interface.GetProperty(name));
                    if (result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        internal static Type FindTypeImplementing(this IEnumerable<Type> types, MethodInfo method)
        {
            Type result = null;
            if (method.DeclaringType.IsInterface)
            {
                result = (
                    from implementingType in types
                    where !implementingType.IsEnum && !implementingType.IsInterface
                    let map = implementingType.GetInterfaceMap(method.DeclaringType)
                    let index = Array.IndexOf(map.InterfaceMethods, method)
                    where index != -1 && !map.TargetMethods[index].IsAbstract
                    select implementingType)
                    .FirstOrDefault();
            }

            return result;
        }

        internal static bool IsGenericCollection(this Type type)
        {
            return (!type.IsGenericType && NonGenericCollectionTypes.Contains(type))
                   || (type.IsGenericType && GenericCollectionTypes.Contains(type.GetGenericTypeDefinition()));
        }
        
        internal static Type GetBaseCollectionType(this Type type)
        {
            if (type.IsGenericType)
            {
                var result = type.GetGenericTypeDefinition();
                if (GenericCollections.Contains(result))
                {
                    return result;
                }
            }

            return NonGenericCollections.Contains(type) ? type : null;
        }

        internal static bool IsReadOnlyEnumerable(this Type valueType)
        {
            bool result;
            lock (ReadOnlyCollections)
            {
                if (!ReadOnlyCollections.TryGetValue(valueType, out result))
                {
                    ReadOnlyCollections[valueType] = result = valueType.GetAddMethod() == null;
                }
            }

            return result;
        }

        internal static void AddEnumerationValue(this Type valueType, IEnumerable currentValue, object value)
        {
            var add = valueType.GetAddMethod(currentValue);
            currentValue.Add(add, valueType.IsADictionary(), value);
        }

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", Justification = "String is culture invariant.")]
        internal static void CopyEnumeration(this Type valueType, IEnumerable currentValue, IEnumerable value)
        {
            var add = valueType.GetAddMethod(currentValue);
            bool isDictionary = valueType.IsADictionary();
            foreach (var item in value)
            {
                currentValue.Add(add, isDictionary, item);
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison", Justification = "String is culture invariant.")]
        private static MethodInfo GetAddMethod(this Type valueType, object collection = null)
        {
            var result = (collection != null ? collection.GetType().GetMethod("Add") : valueType.GetMethod("Add"));
            if (result == null)
            {
                var methods = from type in new[] { valueType }.Concat(valueType.GetInterfaces())
                              from method in type.GetRuntimeMethods()
                              where method.Name == "Add" || method.Name.EndsWith(".Add")
                              select method;
                if (collection != null)
                {
                    methods = from method in methods
                              from parameter in method.GetParameters()
                              join genericType in collection.GetType().GenericTypeArguments on parameter.ParameterType equals genericType into parameters
                              select method;
                }

                result = methods.FirstOrDefault();
            }

            return result;
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

        private static object CreateDefaultLiteral(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
