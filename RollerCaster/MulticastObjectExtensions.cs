﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RollerCaster.Reflection;

namespace RollerCaster
{
    /// <summary>Provides useful <see cref="MulticastObject" /> extension methods.</summary>
    public static class MulticastObjectExtensions
    {
        /// <summary>Gets the property value.</summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="multicastObject">Target <see cref="MulticastObject" /> instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// Value of the property in case it is set or <b>default</b> for a given property's value type.
        /// Types based on <see cref="IEnumerable" /> except <see cref="String" /> and <see cref="Byte" />[] are created on the fly.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "There is a need to cover all required scenarios.")]
        public static TProperty GetProperty<TObject, TProperty>(this MulticastObject multicastObject, string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (propertyName.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName));
            }

            return multicastObject.GetProperty<TProperty>(typeof(TObject).FindProperty(propertyName));
        }

        /// <summary>Gets the property value.</summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="multicastObject">Target <see cref="MulticastObject" /> instance.</param>
        /// <param name="propertyInfo">Property to obtain value for.</param>
        /// <returns>
        /// Value of the property in case it is set or <b>default</b> for a given property's value type.
        /// Types based on <see cref="IEnumerable" /> except <see cref="String" /> and <see cref="Byte" />[] are created on the fly.
        /// </returns>
        public static TProperty GetProperty<TProperty>(this MulticastObject multicastObject, PropertyInfo propertyInfo)
        {
            if (multicastObject == null)
            {
                throw new ArgumentNullException(nameof(multicastObject));
            }

            return (TProperty)multicastObject.GetProperty(propertyInfo);
        }

        /// <summary>Sets the property value.</summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="multicastObject">Target <see cref="MulticastObject" /> instance.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to be set.</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "There is a need to cover all required scenarios.")]
        public static void SetProperty<TObject, TProperty>(this MulticastObject multicastObject, string propertyName, TProperty value)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (propertyName.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName));
            }

            multicastObject.SetProperty<TProperty>(typeof(TObject).FindProperty(propertyName), value);
        }

        /// <summary>Sets the property value.</summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="multicastObject">Target <see cref="MulticastObject" /> instance.</param>
        /// <param name="propertyInfo">Property to set value of.</param>
        /// <param name="value">The value to be set.</param>
        public static void SetProperty<TProperty>(this MulticastObject multicastObject, PropertyInfo propertyInfo, TProperty value)
        {
            if (multicastObject == null)
            {
                throw new ArgumentNullException(nameof(multicastObject));
            }

            multicastObject.SetProperty(propertyInfo, value);
        }

        internal static void CloneInternal(this MulticastObject source, MulticastObject result, IDictionary<object, object> visitedObjects)
        {
            foreach (var propertyValue in source.PropertyValues)
            {
                var collectionType = propertyValue.Property.PropertyType.GetBaseCollectionType();
                if (collectionType != null)
                {
                    if (propertyValue.Property.PropertyType.IsADictionary())
                    {
                        source.CloneDictionary(result, propertyValue, visitedObjects);
                        continue;
                    }

                    source.CloneCollection(result, propertyValue, visitedObjects);
                    continue;
                }

                var newValue = source.CloneValue(propertyValue.Property.PropertyType, propertyValue.Value, visitedObjects);
                result.SetProperty(propertyValue.Property, newValue);
            }
        }

        private static void CloneDictionary(this MulticastObject source, MulticastObject result, MulticastPropertyValue propertyValue, IDictionary<object, object> visitedObjects)
        {
            var sourceDictionary = (IDictionary)propertyValue.Value;
            foreach (var entry in sourceDictionary)
            {
                var newKey = source.CloneValue(propertyValue.Property.PropertyType, entry.GetType().GetProperty("Key").GetValue(entry), visitedObjects);
                var newValue = source.CloneValue(propertyValue.Property.PropertyType, entry.GetType().GetProperty("Value").GetValue(entry), visitedObjects);
                result.SetProperty(propertyValue.Property, new DictionaryEntry(newKey, newValue));
            }
        }

        private static void CloneCollection(this MulticastObject source, MulticastObject result, MulticastPropertyValue propertyValue, IDictionary<object, object> visitedObjects)
        {
            var sourceCollection = (ICollection)propertyValue.Value;
            foreach (var value in sourceCollection)
            {
                var newValue = source.CloneValue(propertyValue.Property.PropertyType, value, visitedObjects);
                result.SetProperty(propertyValue.Property, newValue);
            }
        }

        private static object CloneValue(this MulticastObject source, Type propertyType, object value, IDictionary<object, object> visitedObjects)
        {
            if ((propertyType.IsValueType) || (value == null))
            {
                return value;
            }

            object targetValue = value;
            Type originalType = null;
            MulticastObject multicastObject;
            if (value.TryUnwrap(out multicastObject))
            {
                targetValue = multicastObject;
                originalType = (value as IProxy).CurrentCastedType;
            }

            object instance = CloneObject(source, targetValue, visitedObjects);
            multicastObject = instance as MulticastObject;
            if (multicastObject != null)
            {
                instance = multicastObject.ActLike(originalType);
            }

            return instance;
        }

        private static object CloneObject(this MulticastObject source, object targetValue, IDictionary<object, object> visitedObjects)
        {
            var multicastObject = targetValue as MulticastObject;
            object instance;
            if (visitedObjects.TryGetValue(targetValue, out instance))
            {
                return instance;
            }

            if (multicastObject != null)
            {
                var newMulticastObject = source.CreateChildMulticastObject();
                visitedObjects.Add(multicastObject, instance = newMulticastObject);
                CloneInternal(multicastObject, newMulticastObject, visitedObjects);
                return instance;
            }

            var clonable = targetValue as ICloneable;
            visitedObjects.Add(targetValue, instance = clonable?.Clone() ?? targetValue);
            return instance;
        }
    }
}
