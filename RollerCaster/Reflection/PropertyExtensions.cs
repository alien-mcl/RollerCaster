using System.Linq;
using System.Reflection;

namespace RollerCaster.Reflection
{
    internal static class PropertyExtensions
    {
        internal static bool UseBaseImplementation(this PropertyInfo propertyInfo)
        {
            return !propertyInfo.DeclaringType.IsInterface
                && !propertyInfo.GetAccessors().Any(_ => _.IsAbstract)
                && !propertyInfo.PropertyType.IsAnEnumerable();
        }
    }
}
