using System.Dynamic;

namespace RollerCaster
{
    /// <summary>Base class of the proxy object.</summary>
    internal class ProxyBase : DynamicObject
    {
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (this as IProxy).WrappedObject.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (this as IProxy).WrappedObject.GetHashCode();
        }

        /// <inheritdoc />
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return (this as IProxy).WrappedObject.TryGetMember(binder, out result);
        }

        /// <inheritdoc />
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return (this as IProxy).WrappedObject.TrySetMember(binder, value);
        }
    }
}
