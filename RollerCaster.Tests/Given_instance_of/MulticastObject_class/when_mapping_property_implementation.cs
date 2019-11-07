using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Data;
using RollerCaster.Reflection;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_mapping_property_implementation : MulticastObjectTest
    {
        private static readonly ParameterExpression Parameter = Expression.Parameter(typeof(IMethodCarrier), "_");

        private static readonly Expression<Func<IMethodCarrier, bool>> PropertyToImplement =
            Expression.Lambda<Func<IMethodCarrier, bool>>(
                Expression.MakeMemberAccess(Parameter, typeof(IMethodCarrier).GetProperty(nameof(IMethodCarrier.Property))),
                Parameter);

        private static readonly Expression<Func<IMethodCarrier, bool>> InvalidPropertyToImplement =
            Expression.Lambda<Func<IMethodCarrier, bool>>(
                Expression.MakeMemberAccess(Parameter, typeof(IMethodCarrier).GetProperty(nameof(IMethodCarrier.InvalidProperty))),
                Parameter);

        private static readonly Expression<Func<IMethodCarrier, bool>> ImplementationMethod =
            Expression.Lambda<Func<IMethodCarrier, bool>>(
                Expression.Call(typeof(MethodCarrierImplementation).GetMethod(nameof(MethodCarrierImplementation.Property)), Parameter),
                Parameter);

        private static readonly Expression<Func<IMethodCarrier, bool>> InvalidImplementationMethod =
            Expression.Lambda<Func<IMethodCarrier, bool>>(
                Expression.Call(
                    Expression.Constant(null, typeof(MethodCarrierImplementation)),
                    typeof(MethodCarrierImplementation).GetMethod(nameof(MethodCarrierImplementation.InvalidProperty)),
                    Parameter),
                Parameter);

        [Test]
        public void Should_map_property_correctly()
        {
            new MethodImplementationBuilder<IMethodCarrier>(new Dictionary<MethodInfo, MethodInfo>(), new Dictionary<PropertyInfo, MethodInfo>())
                .ForProperty(PropertyToImplement).ImplementedBy(ImplementationMethod)
                .PropertyImplementationDelegates.Should().HaveCount(1)
                .And.ContainKey((PropertyInfo)((MemberExpression)PropertyToImplement.Body).Member)
                .WhichValue.As<object>().Should().Be(((MethodCallExpression)ImplementationMethod.Body).Method);
        }

        [Test]
        public void Should_throw_on_invalid_implementation_method()
        {
            new MethodImplementationBuilder<IMethodCarrier>(new Dictionary<MethodInfo, MethodInfo>(), new Dictionary<PropertyInfo, MethodInfo>())
                .Invoking(_ => _.ForProperty(PropertyToImplement).ImplementedBy(InvalidImplementationMethod))
                .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Should_throw_on_invalid_property()
        {
            new MethodImplementationBuilder<IMethodCarrier>(new Dictionary<MethodInfo, MethodInfo>(), new Dictionary<PropertyInfo, MethodInfo>())
                .Invoking(_ => _.ForProperty(InvalidPropertyToImplement))
                .Should().Throw<InvalidOperationException>();
        }
    }
}
