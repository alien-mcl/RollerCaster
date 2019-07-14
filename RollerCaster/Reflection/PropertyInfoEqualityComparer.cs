using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RollerCaster.Reflection
{
    /// <summary>Provides a fuzzy logic for <see cref="PropertyInfo" /> equality comparison.</summary>
    public class PropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
    {
        /// <summary>Provides a default instance of the <see cref="PropertyInfoEqualityComparer" />.</summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Class is immutable.")]
        public static readonly PropertyInfoEqualityComparer Default = new PropertyInfoEqualityComparer();

        private PropertyInfoEqualityComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            return (Object.ReferenceEquals(x, null) && Object.ReferenceEquals(y, null))
                   || (!Object.ReferenceEquals(x, null) && !Object.ReferenceEquals(y, null) && (x == y || EqualsInternal(x, y)));
        }

        /// <inheritdoc />
        public int GetHashCode(PropertyInfo obj)
        {
            var result = 0;
            if (obj != null)
            {
                result = obj.PropertyType.GetHashCode() ^ obj.Name.GetHashCode();
                if (obj.DeclaringType != null)
                {
                    result ^= obj.DeclaringType.GetHashCode();
                }
            }

            return result;
        }

        private static bool EqualsInternal(PropertyInfo x, PropertyInfo y)
        {
            return x.DeclaringType == y.DeclaringType && x.PropertyType == y.PropertyType && x.Name == y.Name;
        }
    }
}
