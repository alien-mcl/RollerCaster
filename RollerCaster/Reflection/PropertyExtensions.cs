using System.Reflection;
using System.Runtime.CompilerServices;

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

        internal static bool UseBaseImplementation(this PropertyInfo propertyInfo, bool useSetter = false)
        {
            MethodInfo element = useSetter ? propertyInfo.GetSetMethod() : propertyInfo.GetGetMethod();
            if (propertyInfo.DeclaringType.IsClass && element != null && !element.IsAbstract)
            {
                return element.GetCustomAttribute<CompilerGeneratedAttribute>() == null;
            }

            return false;
        }
    }
}
