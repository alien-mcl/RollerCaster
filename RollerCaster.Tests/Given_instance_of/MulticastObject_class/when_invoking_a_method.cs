using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;
using RollerCaster.Reflection;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_invoking_a_method
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "This is a test subject only.")]
        public static void Action(IInterfaceWithMethod that, int value)
        {
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "This is a test subject only.")]
        public static int Function(IInterfaceWithMethod that, int value)
        {
            return value;
        }

        [SetUp]
        public void Setup()
        {
            WithMethodImplementations(MulticastObject.ImplementationOf<IInterfaceWithMethod>());
        }

        [Test]
        public void Should_call_provided_method()
        {
            new MulticastObject().ActLike<IInterfaceWithMethod>()
                .Invoking(_ => _.Action(new Random((int)DateTime.UtcNow.Ticks).Next()))
                .Should().NotThrow();
        }
        
        [Test]
        public void Should_return_value()
        {
            new MulticastObject().ActLike<IInterfaceWithMethod>()
                .Function(new Random((int)DateTime.UtcNow.Ticks).Next()).Should().NotBe(0);
        }

        private static void WithMethodImplementations(MethodImplementationBuilder<IInterfaceWithMethod> config)
        {
            config
                .ForAction(_ => _.Action(0)).ImplementedBy(_ => Action(_, 0))
                .ForFunction(_ => _.Function(0)).ImplementedBy(_ => Function(_, 0));
        }
    }
}
