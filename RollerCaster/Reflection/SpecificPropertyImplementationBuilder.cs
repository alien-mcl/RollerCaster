using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace RollerCaster.Reflection
{
    /// <summary>Allows to map an implemented property.</summary>
    /// <typeparam name="T">Type of the entity that implements a property.</typeparam>
    /// <typeparam name="TResult">Type of the property.</typeparam>
    public class SpecificPropertyImplementationBuilder<T, TResult>
    {
        private readonly MethodImplementationBuilder<T> _owner;
        
        internal SpecificPropertyImplementationBuilder(MethodImplementationBuilder<T> owner, Expression<Func<T, TResult>> propertyToImplement)
        {
            _owner = owner;
            PropertyToImplement = (PropertyInfo)((MemberExpression)propertyToImplement.Body).Member;
            if (!PropertyToImplement.CanRead || PropertyToImplement.CanWrite)
            {
                throw new InvalidOperationException(
                    $"Unable to implement '{PropertyToImplement.Name}' as only properties with getter can have a custom implementation.");
            }
        }

        internal PropertyInfo PropertyToImplement { get; }

        /// <summary>Allows to map a property's getter implementation.</summary>
        /// <param name="implementationMethod">Property implementation.</param>
        /// <returns>Method implementation builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is part of a fluent-like API and strong typing is essential.")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is part of a fluent-like API and strong typing is essential.")]
        public MethodImplementationBuilder<T> ImplementedBy(Expression<Func<T, TResult>> implementationMethod)
        {
            if (implementationMethod != null)
            {
                ValidatePropertiesAndAdd(((MethodCallExpression)implementationMethod.Body).Method);
            }

            return _owner;
        }

        internal void ValidatePropertiesAndAdd(MethodInfo implementationMethod)
        {
            if (!implementationMethod.IsPublic || !implementationMethod.IsStatic
                || PropertyToImplement.PropertyType != implementationMethod.ReturnType
                || implementationMethod.GetParameters().Length != 1
                || !PropertyToImplement.DeclaringType.IsAssignableFrom(implementationMethod.GetParameters()[0].ParameterType))
            {
                throw new InvalidOperationException($"Unable to implement '{PropertyToImplement.Name}' with '{implementationMethod.Name}.");
            }

            _owner.PropertyImplementationDelegates.Add(PropertyToImplement, implementationMethod);
        }
    }
}