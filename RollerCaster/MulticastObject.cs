using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using RollerCaster.Collections;
using RollerCaster.Reflection;

namespace RollerCaster
{
    /// <summary>Representation of a multi-cast object.</summary>
    public class MulticastObject : DynamicObject, ICloneable
    {
        private static readonly Type ReferenceType = typeof(void);

        /// <summary>Initializes a new instance of the <see cref="MulticastObject"/> class.</summary>
        public MulticastObject()
        {
            Sync = new Object();
            Properties = new Dictionary<Type, Dictionary<Type, Dictionary<PropertyInfo, object>>>();
            Types = new HashSet<Type>();
        }

        /// <summary>Gets casted types.</summary>
        public virtual ICollection<Type> CastedTypes { get { return Types; } }

        /// <summary>Gets a collection of property values.</summary>
        public virtual IEnumerable<MulticastPropertyValue> PropertyValues
        {
            get { return new MulticastPropertyValueCollection(this); }
        }

        internal static IDictionary<MethodInfo, MethodInfo> MethodImplementations { get; }
            = new ConcurrentDictionary<MethodInfo, MethodInfo>();

        internal Dictionary<Type, Dictionary<Type, Dictionary<PropertyInfo, object>>> Properties { get; }

        internal HashSet<Type> Types { get; }

        /// <summary>Gets the multi-threading synchronization context used by this instance.</summary>
        protected object Sync { get; }

        /// <summary>Starts a method implementation mapping.</summary>
        /// <typeparam name="T">Type to map with method implementations.</typeparam>
        /// <returns>Method implementation builder.</returns>
        public static MethodImplementationBuilder<T> ImplementationOf<T>()
        {
            var map = new ObservableDictionary<MethodInfo, MethodInfo>();
            map.CollectionChanged += OnMethodImplementationAdded;
            return new MethodImplementationBuilder<T>(map);
        }

        /// <summary>Gets the property value.</summary>
        /// <param name="propertyInfo">Property to obtain value of.</param>
        /// <returns>
        /// Value of the property in case it is set or <b>default</b> for a given property's value type.
        /// Types based on <see cref="IEnumerable" /> except <see cref="String" /> and <see cref="Byte" />[] are created on the fly.
        /// </returns>
        public virtual object GetProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            var result = GetPhysicalProperty(propertyInfo.Name);
            if (result != null)
            {
                return result;
            }

            this.EnsureDetailsOf(propertyInfo.DeclaringType);
            var valueType = propertyInfo.PropertyType;
            if (valueType.IsAnEnumerable() && valueType.IsGenericCollection())
            {
                return GetEnumerableProperty(valueType, propertyInfo);
            }

            if (!valueType.GetTypeInfo().IsValueType)
            {
                valueType = ReferenceType;
            }

            lock (Sync)
            {
                bool success;
                bool isDefaultValueProvided = false;
                object value = Properties
                    .TryGet(propertyInfo.DeclaringType, out success)
                    .TryGet(valueType, out success)
                    .TryGet(propertyInfo, out success);
                if (!success)
                {
                    isDefaultValueProvided = true;
                    value = propertyInfo.PropertyType.GetDefaultValue();
                }

                if (isDefaultValueProvided && value != null)
                {
                    SetProperty(propertyInfo, value);
                }

                return value;
            }
        }

        /// <summary>Sets the property value.</summary>
        /// <param name="propertyInfo">Property to set value of.</param>
        /// <param name="value">The value to be set.</param>
        public virtual void SetProperty(PropertyInfo propertyInfo, object value)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            if (SetPhysicalProperty(propertyInfo.Name, value))
            {
                return;
            }

            this.EnsureDetailsOf(propertyInfo.DeclaringType);
            var valueType = propertyInfo.PropertyType;
            if (valueType.IsAnEnumerable() && valueType.IsGenericCollection())
            {
                SetEnumerableProperty(valueType, propertyInfo, value);
                return;
            }

            if (!valueType.GetTypeInfo().IsValueType)
            {
                valueType = ReferenceType;
            }

            lock (Sync)
            {
                var typeProperties = Properties
                    .TryGet(propertyInfo.DeclaringType, CreateNewTypePropertyBag)
                    .TryGet(valueType, CreateNewPropertyBag);
                if (value == null)
                {
                    typeProperties.Remove(propertyInfo);
                }
                else
                {
                    typeProperties[propertyInfo] = value;
                }
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method is supposed to try do something. Suppression is OK here.")]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var output = false;
            result = null;
            var propertyInfo = FindUsedPropertyByName(binder?.Name);
            if (propertyInfo != null)
            {
                try
                {
                    result = GetProperty(propertyInfo);
                    output = true;
                }
                catch
                {
                    //// Suppress any exceptions as we try to get a member.
                }
            }

            return output;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method is supposed to try do something. Suppression is OK here.")]
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var output = false;
            var propertyInfo = (
                from type in DynamicExtensions.TypeProperties
                from property in type.Value
                where property.Name == binder.Name
                select property).FirstOrDefault()
                ?? FindUsedPropertyByName(binder.Name);
            if (propertyInfo != null)
            {
                try
                {
                    SetProperty(propertyInfo, value);
                    output = true;
                }
                catch
                {
                    //// Suppress any exceptions as we try to get a member.
                }
            }

            return output;
        }

        /// <summary>Clones this instance.</summary>
        /// <remarks>
        /// When deep-copying, only <see cref="MulticastObject" /> instances are deep copied.
        /// If other instances are implementing <b>"ICloneable</b>, their implementations will be used;
        /// otherwise same instances are taken.</remarks>
        /// <param name="deepClone">Value indicating whether to make a deep copy.</param>
        /// <returns>Copy of this instance.</returns>
        public MulticastObject Clone(bool deepClone = false)
        {
            var result = CreateChildInstance();
            if (!deepClone)
            {
                foreach (var propertyValue in PropertyValues)
                {
                    result.SetProperty(propertyValue.Property, propertyValue.Value);
                }

                return result;
            }

            var visitedObjects = new Dictionary<object, object>();
            visitedObjects[this] = result;
            this.CloneInternal(result, visitedObjects);
            return result;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("UnitTests", "TS000:NoUnitTests", Justification = "Method is a standard implemented wrapper around a tested method that has the logic.")]
        object ICloneable.Clone()
        {
            return Clone(true);
        }

        internal object GetProperty(Type objectType, Type propertyType, string propertyName)
        {
            return GetProperty(objectType.FindProperty(propertyName, propertyType));
        }

        internal void SetProperty(Type objectType, Type propertyType, string propertyName, object value)
        {
            SetProperty(objectType.FindProperty(propertyName, propertyType), value);
        }

        internal MulticastObject CreateChildMulticastObject()
        {
            return CreateChildInstance();
        }

        /// <summary>Creates a new child instance used for clones.</summary>
        /// <returns>Empty instance ready for cloned values.</returns>
        protected virtual MulticastObject CreateChildInstance()
        {
            return new MulticastObject();
        }
        
        private static void OnMethodImplementationAdded(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (KeyValuePair<MethodInfo, MethodInfo> entry in e.NewItems)
                {
                    if (!MethodImplementations.ContainsKey(entry.Key))
                    {
                        MethodImplementations.Add(entry.Key, entry.Value);
                    }
                }
            }
        }

        private static Dictionary<Type, Dictionary<PropertyInfo, object>> CreateNewTypePropertyBag()
        {
            return new Dictionary<Type, Dictionary<PropertyInfo, object>>();
        }

        private static Dictionary<PropertyInfo, object> CreateNewPropertyBag()
        {
            return new Dictionary<PropertyInfo, object>();
        }

        private object GetPhysicalProperty(string propertyName)
        {
            var existingProperty = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if ((existingProperty != null) && (existingProperty.CanRead))
            {
                return existingProperty.GetValue(this);
            }

            return null;
        }

        private bool SetPhysicalProperty(string propertyName, object value)
        {
            var existingProperty = GetType().GetProperty(propertyName);
            if ((existingProperty == null) || (!existingProperty.CanWrite))
            {
                return false;
            }

            existingProperty.SetValue(this, value);
            return true;
        }

        private object GetEnumerableProperty(Type valueType, PropertyInfo propertyInfo)
        {
            lock (Sync)
            {
                return Properties
                    .TryGet(propertyInfo.DeclaringType, CreateNewTypePropertyBag)
                    .TryGet(ReferenceType, CreateNewPropertyBag)
                    .TryGet(propertyInfo, () => valueType.GetDefaultValue());
            }
        }

        private void SetEnumerableProperty(Type valueType, PropertyInfo propertyInfo, object value)
        {
            lock (Sync)
            {
                var currentValue = Properties
                   .TryGet(propertyInfo.DeclaringType, CreateNewTypePropertyBag)
                   .TryGet(ReferenceType, CreateNewPropertyBag)
                   .TryGet(propertyInfo, () => valueType.GetDefaultValue());
                var enumerable = (IEnumerable)currentValue;
                var currentValueType = currentValue.GetType().GetItemType();
                if ((value != null) && (currentValueType.IsInstanceOfType(value)))
                {
                    valueType.AddEnumerationValue(enumerable, value);
                    return;
                }

                currentValue.GetType().GetMethod("Clear").Invoke(currentValue, null);
                if (value != null)
                {
                    valueType.CopyEnumeration(enumerable, (IEnumerable)value);
                }
            }
        }

        private PropertyInfo FindUsedPropertyByName(string name)
        {
            return (
                from type in Properties
                from alignment in type.Value
                from property in alignment.Value
                where property.Key.Name == name
                select property.Key).FirstOrDefault();
        }
    }
}
