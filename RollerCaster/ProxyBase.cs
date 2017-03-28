namespace RollerCaster
{
    /// <summary>Base class of the proxy object.</summary>
    internal class ProxyBase
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
    }
}
