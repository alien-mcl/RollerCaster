using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using RollerCaster.Reflection;

namespace RollerCaster
{
    /// <summary>Representation of a multi-cast object.</summary>
    public class MulticastObject : DynamicObject
#if !NETSTANDARD1_4
#pragma warning disable SA1001 // Commas must be spaced correctly
        ,ICloneable
#pragma warning restore SA1001 // Commas must be spaced correctly
#endif
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
        public virtual IEnumerable<MulticastPropertyValue> PropertyValues { get { return new MulticastPropertyValueCollection(this); } }

        internal Dictionary<Type, Dictionary<Type, Dictionary<PropertyInfo, object>>> Properties { get; }

        internal HashSet<Type> Types { get; }

        /// <summary>Gets the multi-threading synchronization context used by this instance.</summary>
        protected object Sync { get; }

        /// <summary>Gets the property value.</summary>
        /// <param name="objectType">The type of the object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// Value of the property in case it is set or <b>default</b> for a given property's value type.
        /// Types based on <see cref="IEnumerable" /> except <see cref="String" /> and <see cref="Byte" />[] are created on the fly.
        /// </returns>
        public virtual object GetProperty(Type objectType, string propertyName)
        {
            ValidateArguments(objectType, propertyName);
            Types.Add(objectType);
            return GetPhysicalProperty(propertyName) ?? GetProperty(objectType, objectType.FindProperty(propertyName));
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

            Types.Add(propertyInfo.DeclaringType);
            return GetPhysicalProperty(propertyInfo.Name) ?? GetProperty(propertyInfo.DeclaringType, propertyInfo);
        }

        /// <summary>Sets the property value.</summary>
        /// <param name="objectType">Type of the entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to be set.</param>
        public virtual void SetProperty(Type objectType, string propertyName, object value)
        {
            ValidateArguments(objectType, propertyName);
            Types.Add(objectType);
            if (!SetPhysicalProperty(propertyName, value))
            {
                SetProperty(objectType, objectType.FindProperty(propertyName), value);
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

            Types.Add(propertyInfo.DeclaringType);
            if (!SetPhysicalProperty(propertyInfo.Name, value))
            {
                SetProperty(propertyInfo.DeclaringType, propertyInfo, value);
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method is supposed to try do something. Suppression is OK here.")]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            var propertyInfo = (
                from type in Properties
                from alignment in type.Value
                from property in alignment.Value
                where property.Key.Name == binder.Name
                select property.Key).FirstOrDefault();
            if (propertyInfo == null)
            {
                return false;
            }

            try
            {
                result = GetProperty(propertyInfo);
                return true;
            }
            catch
            {
                //// Suppress any exceptions as we try to get a member.
            }

            return false;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method is supposed to try do something. Suppression is OK here.")]
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var propertyInfo = (
                from type in Properties
                from alignment in type.Value
                from property in alignment.Value
                where property.Key.Name == binder.Name
                select property.Key).FirstOrDefault();
            if (propertyInfo == null)
            {
                return false;
            }

            try
            {
                SetProperty(propertyInfo, value);
                return true;
            }
            catch
            {
                //// Suppress any exceptions as we try to get a member.
            }

            return false;
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
                    result.SetProperty(propertyValue.CastedType, propertyValue.Property.Name, propertyValue.Value);
                }

                return result;
            }

            var visitedObjects = new Dictionary<object, object>();
            visitedObjects[this] = result;
            this.CloneInternal(result, visitedObjects);
            return result;
        }

#if !NETSTANDARD1_4
        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("UnitTests", "TS000:NoUnitTests", Justification = "Method is a standard implemented wrapper around a tested method that has the logic.")]
        object ICloneable.Clone()
        {
            return Clone(true);
        }
#endif

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

        private static void ValidateArguments(Type objectType, string propertyName)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (propertyName.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName));
            }
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

        private object GetProperty(Type objectType, PropertyInfo propertyInfo)
        {
            var valueType = propertyInfo.PropertyType;
            if (valueType.IsAnEnumerable())
            {
                return GetEnumerableProperty(objectType, valueType, propertyInfo);
            }

            if (!valueType.GetTypeInfo().IsValueType)
            {
                valueType = ReferenceType;
            }

            lock (Sync)
            {
                Dictionary<Type, Dictionary<PropertyInfo, object>> entityTypeProperties;
                if (!Properties.TryGetValue(objectType, out entityTypeProperties))
                {
                    return propertyInfo.PropertyType.GetDefaultValue();
                }

                Dictionary<PropertyInfo, object> typeProperties;
                if (!entityTypeProperties.TryGetValue(valueType, out typeProperties))
                {
                    return propertyInfo.PropertyType.GetDefaultValue();
                }

                object value;
                return (!typeProperties.TryGetValue(propertyInfo, out value) ? propertyInfo.PropertyType.GetDefaultValue() : value);
            }
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

        private void SetProperty(Type objectType, PropertyInfo propertyInfo, object value)
        {
            var valueType = propertyInfo.PropertyType;
            if (valueType.IsAnEnumerable())
            {
                SetEnumerableProperty(objectType, valueType, propertyInfo, value);
                return;
            }

            if (!valueType.GetTypeInfo().IsValueType)
            {
                valueType = ReferenceType;
            }

            lock (Sync)
            {
                Dictionary<Type, Dictionary<PropertyInfo, object>> entityTypeProperties;
                if (!Properties.TryGetValue(objectType, out entityTypeProperties))
                {
                    Properties[objectType] = entityTypeProperties = new Dictionary<Type, Dictionary<PropertyInfo, object>>();
                }

                Dictionary<PropertyInfo, object> typeProperties;
                if (!entityTypeProperties.TryGetValue(valueType, out typeProperties))
                {
                    entityTypeProperties[valueType] = typeProperties = new Dictionary<PropertyInfo, object>();
                }

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

        private object GetEnumerableProperty(Type objectType, Type valueType, PropertyInfo propertyInfo)
        {
            lock (Sync)
            {
                Dictionary<Type, Dictionary<PropertyInfo, object>> entityTypeProperties;
                if (!Properties.TryGetValue(objectType, out entityTypeProperties))
                {
                    Properties[objectType] = entityTypeProperties = new Dictionary<Type, Dictionary<PropertyInfo, object>>();
                }

                Dictionary<PropertyInfo, object> typeProperties;
                if (!entityTypeProperties.TryGetValue(ReferenceType, out typeProperties))
                {
                    entityTypeProperties[ReferenceType] = typeProperties = new Dictionary<PropertyInfo, object>();
                }

                object value;
                if (!typeProperties.TryGetValue(propertyInfo, out value))
                {
                    typeProperties[propertyInfo] = value = valueType.GetDefaultValue();
                }

                return value;
            }
        }

        private void SetEnumerableProperty(Type objectType, Type valueType, PropertyInfo propertyInfo, object value)
        {
            lock (Sync)
            {
                Dictionary<Type, Dictionary<PropertyInfo, object>> entityTypeProperties;
                if (!Properties.TryGetValue(objectType, out entityTypeProperties))
                {
                    Properties[objectType] = entityTypeProperties = new Dictionary<Type, Dictionary<PropertyInfo, object>>();
                }

                Dictionary<PropertyInfo, object> typeProperties;
                if (!entityTypeProperties.TryGetValue(ReferenceType, out typeProperties))
                {
                    entityTypeProperties[ReferenceType] = typeProperties = new Dictionary<PropertyInfo, object>();
                }

                object currentValue;
                if (!typeProperties.TryGetValue(propertyInfo, out currentValue))
                {
                    typeProperties[propertyInfo] = currentValue = valueType.GetDefaultValue();
                }

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
    }
}
