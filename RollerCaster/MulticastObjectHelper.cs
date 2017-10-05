using System.Dynamic;

namespace RollerCaster
{
    /// <summary>Base class of the proxy object.</summary>
    internal static class MulticastObjectHelper
    {
        internal static bool Equals(IProxy instance, object obj)
        {
            MulticastObject anotherInstance;
            return (instance.WrappedObject.Equals(obj.TryUnwrap(out anotherInstance) ? anotherInstance : obj));
        }

        internal static int GetHashCode(IProxy instance)
        {
            return instance.WrappedObject.GetHashCode();
        }

        internal static bool TryGetMember(IProxy instance, GetMemberBinder binder, out object result)
        {
            return instance.WrappedObject.TryGetMember(binder, out result);
        }

        internal static bool TrySetMember(IProxy instance, SetMemberBinder binder, object value)
        {
            return instance.WrappedObject.TrySetMember(binder, value);
        }
    }
}
