using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Data;
using RollerCaster.Reflection;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_mapping_method_implementation : MulticastObjectTest
    {
        private static readonly ParameterExpression Parameter = Expression.Parameter(typeof(IMethodCarrier), "_");

        public static IEnumerable<TestCaseData> Actions
        {
            get
            {
                for (int index = 0; index <= 7; index++)
                {
                    yield return CreateTestCase(index, "Action");
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        public static IEnumerable<TestCaseData> Funcs
        {
            get
            {
                for (int index = 0; index <= 7; index++)
                {
                    yield return CreateTestCase(index, "Func");
                }
            }
        }

        public static IEnumerable<TestCaseData> InvalidMatches
        {
            get
            {
                yield return CreateTestCase(0, "Func", "InvalidMethod");
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a test subject only.")]
        [Test]
        [TestCaseSource(nameof(Actions))]
        public void Should_map_action_correctly(
            Expression<Action<IMethodCarrier>> methodToImplement,
            Expression<Action<IMethodCarrier>> implementationMethod)
        {
            new MethodImplementationBuilder<IMethodCarrier>(new Dictionary<MethodInfo, MethodInfo>(), new Dictionary<PropertyInfo, MethodInfo>())
                .ForAction(methodToImplement).ImplementedBy(implementationMethod)
                .MethodImplementationDelegates.Should().HaveCount(1)
                .And.ContainKey(((MethodCallExpression)methodToImplement.Body).Method)
                .WhichValue.As<object>().Should().Be(((MethodCallExpression)implementationMethod.Body).Method);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a test subject only.")]
        [Test]
        [TestCaseSource(nameof(Funcs))]
        public void Should_map_func_correctly(
            Expression<Func<IMethodCarrier, bool>> methodToImplement,
            Expression<Func<IMethodCarrier, bool>> implementationMethod)
        {
            new MethodImplementationBuilder<IMethodCarrier>(new Dictionary<MethodInfo, MethodInfo>(), new Dictionary<PropertyInfo, MethodInfo>())
                .ForFunction(methodToImplement).ImplementedBy(implementationMethod)
                .MethodImplementationDelegates.Should().HaveCount(1)
                .And.ContainKey(((MethodCallExpression)methodToImplement.Body).Method)
                .WhichValue.As<object>().Should().Be(((MethodCallExpression)implementationMethod.Body).Method);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is a test subject only.")]
        [Test]
        [TestCaseSource(nameof(InvalidMatches))]
        public void Should_throw_on_invalid_implementation_method(
            Expression<Func<IMethodCarrier, bool>> methodToImplement,
            Expression<Func<IMethodCarrier, bool>> implementationMethod)
        {
            new MethodImplementationBuilder<IMethodCarrier>(new Dictionary<MethodInfo, MethodInfo>(), new Dictionary<PropertyInfo, MethodInfo>())
                .Invoking(_ => _.ForFunction(methodToImplement).ImplementedBy(implementationMethod))
                .ShouldThrow<InvalidOperationException>();
        }

        private static TestCaseData CreateTestCase(int index, string methodName, string implementationMethodName = null)
        {
            var methodToImplement = typeof(IMethodCarrier)
                .GetMethods().First(_ => _.Name == methodName && _.GetParameters().Length == index);
            var implementationMethod = typeof(MethodCarrierImplementation)
                .GetMethods().First(_ => _.Name == (implementationMethodName ?? methodName) && _.GetParameters().Length == index + 1);
            var parameters = Enumerable.Range(0, index)
                .Select(_ => Expression.Constant(Activator.CreateInstance(methodToImplement.GetParameters()[_].ParameterType)));
            return new TestCaseData(
                Expression.Lambda(CreateCall(Parameter, methodToImplement, parameters), Parameter),
                Expression.Lambda(CreateCall(Parameter, implementationMethod, parameters, implementationMethodName != null ? typeof(string) : null), Parameter));
        }

        private static MethodCallExpression CreateCall(
            ParameterExpression parameter,
            MethodInfo method,
            IEnumerable<Expression> parameters,
            Type instanceParameterType = null)
        {
            if (method.IsStatic)
            {
                var argument = instanceParameterType != null ? Expression.Constant(null, instanceParameterType) : (Expression)Parameter;
                return Expression.Call(method, new[] { argument }.Concat(parameters));
            }
            else
            {
                return Expression.Call(parameter, method, parameters);
            }
        }
    }
}
