#pragma warning disable SA1402 // File may only contain a single class
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace RollerCaster.Reflection
{
    /// <summary>Begins a method implementation mapping.</summary>
    public class MethodImplementationBuilder
    {
        internal MethodImplementationBuilder(
            IDictionary<MethodInfo, MethodInfo> methodsMap,
            IDictionary<PropertyInfo, MethodInfo> propertiesMap)
        {
            MethodImplementationDelegates = methodsMap;
            PropertyImplementationDelegates = propertiesMap;
        }

        internal IDictionary<MethodInfo, MethodInfo> MethodImplementationDelegates { get; }

        internal IDictionary<PropertyInfo, MethodInfo> PropertyImplementationDelegates { get; }
    }

    /// <summary>Begins a method implementation mapping.</summary>
    /// <typeparam name="T">Type of entity being mapped.</typeparam>
    public class MethodImplementationBuilder<T> : MethodImplementationBuilder
    {
        internal MethodImplementationBuilder(
            IDictionary<MethodInfo, MethodInfo> methodsMap,
            IDictionary<PropertyInfo, MethodInfo> propertiesMap)
            : base(methodsMap, propertiesMap)
        {
        }

        /// <summary>Points a method to implement.</summary>
        /// <param name="method">Method to implement.</param>
        /// <returns>Method implementation builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is part of a fluent-like API and strong typing is essential.")]
        public SpecificActionImplementationBuilder<T> ForAction(Expression<Action<T>> method)
        {
            return new SpecificActionImplementationBuilder<T>(this, method);
        }

        /// <summary>Points a method to implement.</summary>
        /// <param name="method">Method to implement.</param>
        /// <typeparam name="TResult">Type of the method result.</typeparam>
        /// <returns>Method implementation builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is part of a fluent-like API and strong typing is essential.")]
        public SpecificFuncImplementationBuilder<T, TResult> ForFunction<TResult>(Expression<Func<T, TResult>> method)
        {
            return new SpecificFuncImplementationBuilder<T, TResult>(this, method);
        }

        /// <summary>Points a property's getter to implement.</summary>
        /// <param name="method">Property to implement.</param>
        /// <typeparam name="TResult">Type of the property.</typeparam>
        /// <returns>Property implementation builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is part of a fluent-like API and strong typing is essential.")]
        public SpecificPropertyImplementationBuilder<T, TResult> ForProperty<TResult>(Expression<Func<T, TResult>> method)
        {
            return new SpecificPropertyImplementationBuilder<T, TResult>(this, method);
        }
    }
}
#pragma warning restore SA1402 // File may only contain a single class
