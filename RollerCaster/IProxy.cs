using System;

namespace RollerCaster
{
    /// <summary>Abstract of the proxy object.</summary>
    public interface IProxy
    {
        /// <summary>Gets the wrapped object.</summary>
        MulticastObject WrappedObject { get; }

        /// <summary>Gets the current type this proxy acts like.</summary>
        Type CurrentCastedType { get; }
    }
}
