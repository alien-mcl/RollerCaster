#if NETSTANDARD1_4
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>Specifies that the attributed code should be excluded from code coverage information. </summary>
    [AttributeUsageAttribute(
        AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method |
        AttributeTargets.Property | AttributeTargets.Event, Inherited = false)]
    public sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
    }
}
#endif