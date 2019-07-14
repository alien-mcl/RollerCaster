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

        internal void ValidateMethodsAndAdd(MethodInfo methodToImplement, MethodInfo implementationMethod)
        {
            if (!implementationMethod.IsPublic || !implementationMethod.IsStatic
                || methodToImplement.ReturnType != implementationMethod.ReturnType
                || methodToImplement.GetParameters().Length + 1 != implementationMethod.GetParameters().Length
                || !methodToImplement.GetParameters().Select(_ => _.ParameterType)
                    .SequenceEqual(implementationMethod.GetParameters().Skip(1).Select(_ => _.ParameterType))
                || !methodToImplement.DeclaringType.IsAssignableFrom(implementationMethod.GetParameters()[0].ParameterType))
            {
                throw new InvalidOperationException($"Unable to implement '{methodToImplement.Name}' with '{implementationMethod.Name}.");
            }

            _owner.ImplementationDelegates.Add(MethodToImplement, implementationMethod);
        }
    }
}
