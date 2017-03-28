using System;
using System.Diagnostics;
using System.Reflection;

namespace RollerCaster
{
    /// <summary>Describes a property set on a given multi-cast object.</summary>
    [DebuggerDisplay("{CastedType.Name,nq}.{Property.Name,nq} = {Value,nq}")]
    public class MulticastPropertyValue
    {
        /// <summary>Initializes a new instance of the <see cref="MulticastPropertyValue" /> class.</summary>
        /// <param name="castedType">Type entity was casted to when the property is being set.</param>
        /// <param name="property">Property being set.</param>
        /// <param name="value">Value being set.</param>
        public MulticastPropertyValue(Type castedType, PropertyInfo property, object value)
        {
            CastedType = castedType;
            Property = property;
            Value = value;
        }

        /// <summary>Gets the casted type.</summary>
        public Type CastedType { get; }

        /// <summary>Gets property set.</summary>
        public PropertyInfo Property { get; }

        /// <summary>Gets the value.</summary>
        public object Value { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            MulticastPropertyValue anotherPropertyValue = obj as MulticastPropertyValue;
            return (ReferenceEquals(this, obj)) || ((anotherPropertyValue != null) &&
                (CastedType == anotherPropertyValue.CastedType) && (Property == anotherPropertyValue.Property) && (Equals(Value, anotherPropertyValue.Value)));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return CastedType.GetHashCode() ^ Property.GetHashCode() ^ Value?.GetHashCode() ?? 0;
        }
    }
}
