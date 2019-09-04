using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RollerCaster.Reflection
{
    /// <summary>Allows to map an implemented method.</summary>
    public class SpecificMethodImplementationBuilder
    {
        private readonly MethodImplementationBuilder _owner;

        internal SpecificMethodImplementationBuilder(MethodImplementationBuilder owner, LambdaExpression methodToImplement)
        {
            _owner = owner;
            MethodToImplement = ((MethodCallExpression)methodToImplement.Body).Method;
        }

        internal MethodInfo MethodToImplement { get; }

        internal void ValidateMethodsAndAdd(MethodInfo implementationMethod)
        {
            if (!implementationMethod.IsPublic || !implementationMethod.IsStatic
                || MethodToImplement.ReturnType != implementationMethod.ReturnType
                || MethodToImplement.GetParameters().Length + 1 != implementationMethod.GetParameters().Length
                || !MethodToImplement.GetParameters().Select(_ => _.ParameterType)
                    .SequenceEqual(implementationMethod.GetParameters().Skip(1).Select(_ => _.ParameterType))
                || !MethodToImplement.DeclaringType.IsAssignableFrom(implementationMethod.GetParameters()[0].ParameterType))
            {
                throw new InvalidOperationException($"Unable to implement '{MethodToImplement.Name}' with '{implementationMethod.Name}.");
            }

            _owner.MethodImplementationDelegates.Add(MethodToImplement, implementationMethod);
        }
    }
}
