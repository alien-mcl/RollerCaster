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
    {
        private static readonly Type ReferenceType = typeof(void);

        /// <summary>Initializes a new instance of the <see cref="MulticastObject"/> class.</summary>
        public MulticastObject()
        {
            Sync = new Object();
            Properties = new Dictionary<Type, Dictionary<Type, Dictionary<string, object>>>();
            Types = new HashSet<Type>();
        }

        /// <summary>Gets casted types.</summary>
        public virtual ICollection<Type> CastedTypes { get { return Types; } }

        /// <summary>Gets a collection of property values.</summary>
        public virtual IEnumerable<MulticastPropertyValue> PropertyValues { get { return new MulticastPropertyValueCollection(this); } }

        internal Dictionary<Type, Dictionary<Type, Dictionary<string, object>>> Properties { get; }

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
            var existingProperty = GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if ((existingProperty != null) && (existingProperty.CanRead))
            {
                return existingProperty.GetValue(this);
            }

            var propertyType = objectType.GetProperty(propertyName).PropertyType;
            var valueType = propertyType;
            if (valueType.IsAnEnumerable())
            {
                return GetEnumerableProperty(objectType, valueType, propertyName);
            }

            if (!valueType.GetTypeInfo().IsValueType)
            {
                valueType = ReferenceType;
            }

            lock (Sync)
            {
                Dictionary<Type, Dictionary<string, object>> entityTypeProperties;
                if (!Properties.TryGetValue(objectType, out entityTypeProperties))
                {
                    return propertyType.GetDefaultValue();
                }

                Dictionary<string, object> typeProperties;
                if (!entityTypeProperties.TryGetValue(valueType, out typeProperties))
                {
                    return propertyType.GetDefaultValue();
                }

                object value;
                return (!typeProperties.TryGetValue(propertyName, out value) ? propertyType.GetDefaultValue() : value);
            }
        }

        /// <summary>Sets the property value.</summary>
        /// <param name="objectType">Type of the entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to be set.</param>
        public virtual void SetProperty(Type objectType, string propertyName, object value)
        {
            ValidateArguments(objectType, propertyName);
            Types.Add(objectType);
            var existingProperty = GetType().GetProperty(propertyName);
            if ((existingProperty != null) && (existingProperty.CanWrite))
            {
                existingProperty.SetValue(this, value);
                return;
            }

            var valueType = objectType.GetProperty(propertyName).PropertyType;
            if (valueType.IsAnEnumerable())
            {
                SetEnumerableProperty(objectType, valueType, propertyName, value);
                return;
            }

            if (!valueType.GetTypeInfo().IsValueType)
            {
                valueType = ReferenceType;
            }

            lock (Sync)
            {
                Dictionary<Type, Dictionary<string, object>> entityTypeProperties;
                if (!Properties.TryGetValue(objectType, out entityTypeProperties))
                {
                    Properties[objectType] = entityTypeProperties = new Dictionary<Type, Dictionary<string, object>>();
                }

                Dictionary<string, object> typeProperties;
                if (!entityTypeProperties.TryGetValue(valueType, out typeProperties))
                {
                    entityTypeProperties[valueType] = typeProperties = new Dictionary<string, object>();
                }

                if (value == null)
                {
                    typeProperties.Remove(propertyName);
                }
                else
                {
                    typeProperties[propertyName] = value;
                }
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method is supposed to try do something. Suppression is OK here.")]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            var objectType = (from type in Properties
                              from alignment in type.Value
                              from property in alignment.Value
                              where property.Key == binder.Name
                              select type.Key).FirstOrDefault();
            if (objectType == null)
            {
                return false;
            }

            try
            {
                result = GetProperty(objectType, binder.Name);
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
            var objectType = (from type in Properties
                              from alignment in type.Value
                              from property in alignment.Value
                              where property.Key == binder.Name
                              select type.Key).FirstOrDefault();
            if (objectType == null)
            {
                return false;
            }

            try
            {
                SetProperty(objectType, binder.Name, value);
                return true;
            }
            catch
            {
                //// Suppress any exceptions as we try to get a member.
            }

            return false;
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

        private object GetEnumerableProperty(Type objectType, Type valueType, string propertyName)
        {
            lock (Sync)
            {
                Dictionary<Type, Dictionary<string, object>> entityTypeProperties;
                if (!Properties.TryGetValue(objectType, out entityTypeProperties))
                {
                    Properties[objectType] = entityTypeProperties = new Dictionary<Type, Dictionary<string, object>>();
                }

                Dictionary<string, object> typeProperties;
                if (!entityTypeProperties.TryGetValue(ReferenceType, out typeProperties))
                {
                    entityTypeProperties[ReferenceType] = typeProperties = new Dictionary<string, object>();
                }

                object value;
                if (!typeProperties.TryGetValue(propertyName, out value))
                {
                    typeProperties[propertyName] = value = valueType.GetDefaultValue();
                }

                return value;
            }
        }

        private void SetEnumerableProperty(Type objectType, Type valueType, string propertyName, object value)
        {
            lock (Sync)
            {
                Dictionary<Type, Dictionary<string, object>> entityTypeProperties;
                if (!Properties.TryGetValue(objectType, out entityTypeProperties))
                {
                    Properties[objectType] = entityTypeProperties = new Dictionary<Type, Dictionary<string, object>>();
                }

                Dictionary<string, object> typeProperties;
                if (!entityTypeProperties.TryGetValue(ReferenceType, out typeProperties))
                {
                    entityTypeProperties[ReferenceType] = typeProperties = new Dictionary<string, object>();
                }

                object currentValue;
                if (!typeProperties.TryGetValue(propertyName, out currentValue))
                {
                    typeProperties[propertyName] = currentValue = valueType.GetDefaultValue();
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
