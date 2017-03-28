using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

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
            return multicastObject.GetProperty<TProperty>(typeof(TObject), propertyName);
        }

        /// <summary>Gets the property value.</summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="multicastObject">Target <see cref="MulticastObject" /> instance.</param>
        /// <param name="objectType">The type of the object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// Value of the property in case it is set or <b>default</b> for a given property's value type.
        /// Types based on <see cref="IEnumerable" /> except <see cref="String" /> and <see cref="Byte" />[] are created on the fly.
        /// </returns>
        public static TProperty GetProperty<TProperty>(this MulticastObject multicastObject, Type objectType, string propertyName)
        {
            if (multicastObject == null)
            {
                throw new ArgumentNullException(nameof(multicastObject));
            }

            return (TProperty)multicastObject.GetProperty(objectType, propertyName);
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
            if (multicastObject == null)
            {
                throw new ArgumentNullException(nameof(multicastObject));
            }

            multicastObject.SetProperty<TProperty>(typeof(TObject), propertyName, value);
        }

        /// <summary>Sets the property value.</summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="multicastObject">Target <see cref="MulticastObject" /> instance.</param>
        /// <param name="objectType">Type of the entity.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value to be set.</param>
        public static void SetProperty<TProperty>(this MulticastObject multicastObject, Type objectType, string propertyName, TProperty value)
        {
            if (multicastObject == null)
            {
                throw new ArgumentNullException(nameof(multicastObject));
            }

            multicastObject.SetProperty(objectType, propertyName, (object)value);
        }
    }
}
