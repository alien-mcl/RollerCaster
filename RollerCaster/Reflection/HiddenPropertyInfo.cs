using System;
using System.Globalization;
using System.Reflection;

namespace RollerCaster.Reflection
{
    /// <summary>Provides a fake implementation of the <see cref="PropertyInfo" /> to emulate hidden properties.</summary>
    public class HiddenPropertyInfo : PropertyInfo
    {
        internal const string HiddenPropertyNameIndicator = "_";

        /// <summary>Initializes a new instance of the <see cref="HiddenPropertyInfo" /> class.</summary>
        /// <param name="name">Name of the property.</param>
        /// <param name="propertyType">Tyoe of the property.</param>
        /// <param name="declaringType">Type that declares the property.</param>
        /// <param name="reflectedType">Type that the instance owning this property was.</param>
        public HiddenPropertyInfo(string name, Type propertyType, Type declaringType, Type reflectedType = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name.StartsWith(HiddenPropertyNameIndicator, StringComparison.OrdinalIgnoreCase)
                ? name
                : HiddenPropertyNameIndicator + name;
            PropertyType = propertyType;
            DeclaringType = declaringType;
            ReflectedType = reflectedType ?? declaringType;
        }

        /// <inheritdoc />
        public override Type PropertyType { get; }

        /// <inheritdoc />
        public override PropertyAttributes Attributes { get; } = PropertyAttributes.None;

        /// <inheritdoc />
        public override bool CanRead { get; } = true;

        /// <inheritdoc />
        public override bool CanWrite { get; } = true;

        /// <inheritdoc />
        public override string Name { get; }

        /// <inheritdoc />
        public override Type DeclaringType { get; }

        /// <inheritdoc />
        public override Type ReflectedType { get; }

        /// <inheritdoc />
        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return new[] { GetGetMethod(nonPublic), GetSetMethod(nonPublic) };
        }

        /// <inheritdoc />
        public override object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributes(null, inherit);
        }

        /// <inheritdoc />
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return Array.Empty<object>();
        }

        /// <inheritdoc />
        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return GetType().GetMethod(nameof(GetValueInternal), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <inheritdoc />
        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return GetType().GetMethod(nameof(SetValueInternal), BindingFlags.Instance | BindingFlags.NonPublic);
        }

        /// <inheritdoc />
        public override ParameterInfo[] GetIndexParameters()
        {
            return Array.Empty<ParameterInfo>();
        }

        /// <inheritdoc />
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        /// <inheritdoc />
        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            return GetValueInternal(obj);
        }

        /// <inheritdoc />
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            SetValueInternal(obj, value);
        }

        private object GetValueInternal(object obj)
        {
            MulticastObject proxy;
            return obj.TryUnwrap(out proxy)
                ? proxy.GetProperty(this)
                : (PropertyType.CanHaveNullValue() ? null : Activator.CreateInstance(PropertyType));
        }

        private void SetValueInternal(object obj, object value)
        {
            MulticastObject proxy;
            if (obj.TryUnwrap(out proxy))
            {
                proxy.SetProperty(this, value);
            }
        }
    }
}
