using System.Reflection;

namespace RollerCaster.Reflection
{
    internal static class PropertyExtensions
    {
        internal static bool OverridesBase(this PropertyInfo propertyInfo)
        {
            var baseType = propertyInfo.DeclaringType.BaseType;
            while (baseType != typeof(object))
            {
                if (baseType.GetProperty(propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public) != null)
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }
    }
}
