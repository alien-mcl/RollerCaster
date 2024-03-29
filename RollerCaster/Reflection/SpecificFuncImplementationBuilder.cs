﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace RollerCaster.Reflection
{
    /// <summary>Allows to map an implemented method.</summary>
    /// <typeparam name="T">Type of the entity that implements a method.</typeparam>
    /// <typeparam name="TResult">Type of the method result.</typeparam>
    public class SpecificFuncImplementationBuilder<T, TResult> : SpecificMethodImplementationBuilder
    {
        private readonly MethodImplementationBuilder<T> _owner;

        internal SpecificFuncImplementationBuilder(MethodImplementationBuilder<T> owner, Expression<Func<T, TResult>> methodToImplement)
            : base(owner, methodToImplement)
        {
            _owner = owner;
        }

        /// <summary>Allows to map a method implementation.</summary>
        /// <param name="implementationMethod">Method implementation.</param>
        /// <returns>Method implementation builder.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is part of a fluent-like API and strong typing is essential.")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is part of a fluent-like API and strong typing is essential.")]
        public MethodImplementationBuilder<T> ImplementedBy(Expression<Func<T, TResult>> implementationMethod)
        {
            if (implementationMethod != null)
            {
                ValidateMethodsAndAdd(((MethodCallExpression)implementationMethod.Body).Method);
            }

            return _owner;
        }
    }
}