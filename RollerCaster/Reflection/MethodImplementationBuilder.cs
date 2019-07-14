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
        internal MethodImplementationBuilder(IDictionary<MethodInfo, MethodInfo> map)
        {
            ImplementationDelegates = map;
        }

        internal IDictionary<MethodInfo, MethodInfo> ImplementationDelegates { get; }
    }

    /// <summary>Begins a method implementation mapping.</summary>
    /// <typeparam name="T">Type of entity being mapped.</typeparam>
    public class MethodImplementationBuilder<T> : MethodImplementationBuilder
    {
        internal MethodImplementationBuilder(IDictionary<MethodInfo, MethodInfo> map) : base(map)
        {
        }

        /// <summary>Points a method to implement.</summary>
        /// <param name="method">Method to implement.</param>
        /// <returns>Method implementation builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is part of a fluent-like API and strong typing is essential.")]
        public SpecificActionImplementationBuilder<T> ForAction(
            Expression<Action<T>> method)
        {
            return new SpecificActionImplementationBuilder<T>(this, method);
        }

        /// <summary>Points a method to implement.</summary>
        /// <param name="method">Method to implement.</param>
        /// <typeparam name="TResult">Type of the method result.</typeparam>
        /// <returns>Method implementation builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is part of a fluent-like API and strong typing is essential.")]
        public SpecificFuncImplementationBuilder<T, TResult> ForFunction<TResult>(
            Expression<Func<T, TResult>> method)
        {
            return new SpecificFuncImplementationBuilder<T, TResult>(this, method);
        }
    }
}
#pragma warning restore SA1402 // File may only contain a single class
