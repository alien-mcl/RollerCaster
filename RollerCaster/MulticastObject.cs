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
    public class MulticastObject : DynamicObject, ICloneable
    {
        private static readonly Type ReferenceType = typeof(void);

        /// <summary>Initializes a new instance of the <see cref="MulticastObject"/> class.</summary>
        public MulticastObject()
        {
            Sync = new Object();
            Properties = new Dictionary<Type, Dictionary<Type, Dictionary<PropertyInfo, object>>>();
            Types = new HashSet<Type>();
            TypeProperties = new Dictionary<Type, IEnumerable<PropertyInfo>>();
            TypeInstances = new Dictionary<Type, object>();
        }

        /// <summary>Gets casted types.</summary>
        public virtual ICollection<Type> CastedTypes { get { return Types; } }

        /// <summary>Gets a collection of property values.</summary>
        public virtual IEnumerable<MulticastPropertyValue> PropertyValues
        {
            get { return new MulticastPropertyValueCollection(this); }
        }

        internal Dictionary<Type, Dictionary<Type, Dictionary<PropertyInfo, object>>> Properties { get; }

        internal HashSet<Type> Types { get; }

        internal Dictionary<Type, IEnumerable<PropertyInfo>> TypeProperties { get; }

        internal Dictionary<Type, object> TypeInstances { get; }

        /// <summary>Gets the multi-threading synchronization context used by this instance.</summary>
        protected object Sync { get; }

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

            Types.Add(propertyInfo.DeclaringType);
            var valueType = propertyInfo.PropertyType;
            if (valueType.IsAnEnumerable())
            {
                return GetEnumerableProperty(valueType, propertyInfo);
            }

            if (!valueType.GetTypeInfo().IsValueType)
            {
                valueType = ReferenceType;
            }

            lock (Sync)
            {
                Dictionary<Type, Dictionary<PropertyInfo, object>> entityTypeProperties;
                if (!Properties.TryGetValue(propertyInfo.DeclaringType, out entityTypeProperties))
                {
                    Properties[propertyInfo.DeclaringType] = entityTypeProperties = new Dictionary<Type, Dictionary<PropertyInfo, object>>();
                }

                Dictionary<PropertyInfo, object> typeProperties;
                if (!entityTypeProperties.TryGetValue(valueType, out typeProperties))
                {
                    entityTypeProperties[valueType] = typeProperties = new Dictionary<PropertyInfo, object>();
                }

                object value;
                if (!typeProperties.TryGetValue(propertyInfo, out value))
                {
                    typeProperties[propertyInfo] = value = propertyInfo.PropertyType.GetDefaultValue();
                }

                return value;
            }
        }

        /// <summary>Sets the property value.</summary>
        /// <param name="propertyInfo">Property to set value of.</param>
        /// <param name="value">The value to be set.</param>
        public virtual void SetProperty(PropertyInfo propertyInfo, object value)
        {
            object instance;
            if (propertyInfo == null
                || !propertyInfo.UseBaseImplementation()
                || (!TypeInstances.TryGetValue(propertyInfo.DeclaringType, out instance)
                    && !TypeInstances.TryGetValue(propertyInfo.ReflectedType, out instance)))
            {
                instance = null;
            }

            SetProperty(propertyInfo, value, instance);
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
            SetProperty(objectType.FindProperty(propertyName, propertyType), value, null);
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

        private void SetProperty(PropertyInfo propertyInfo, object value, object instance)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            if (instance != null)
            {
                propertyInfo.SetValue(instance, value);
                return;
            }

            if (SetPhysicalProperty(propertyInfo.Name, value))
            {
                return;
            }

            Types.Add(propertyInfo.DeclaringType);
            var valueType = propertyInfo.PropertyType;
            if (valueType.IsAnEnumerable())
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
                Dictionary<Type, Dictionary<PropertyInfo, object>> entityTypeProperties;
                if (!Properties.TryGetValue(propertyInfo.DeclaringType, out entityTypeProperties))
                {
                    Properties[propertyInfo.DeclaringType] = entityTypeProperties = new Dictionary<Type, Dictionary<PropertyInfo, object>>();
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

        private object GetEnumerableProperty(Type valueType, PropertyInfo propertyInfo)
        {
            lock (Sync)
            {
                Dictionary<Type, Dictionary<PropertyInfo, object>> entityTypeProperties;
                if (!Properties.TryGetValue(propertyInfo.DeclaringType, out entityTypeProperties))
                {
                    Properties[propertyInfo.DeclaringType] = entityTypeProperties = new Dictionary<Type, Dictionary<PropertyInfo, object>>();
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

        private void SetEnumerableProperty(Type valueType, PropertyInfo propertyInfo, object value)
        {
            lock (Sync)
            {
                Dictionary<Type, Dictionary<PropertyInfo, object>> entityTypeProperties;
                if (!Properties.TryGetValue(propertyInfo.DeclaringType, out entityTypeProperties))
                {
                    Properties[propertyInfo.DeclaringType] = entityTypeProperties = new Dictionary<Type, Dictionary<PropertyInfo, object>>();
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
