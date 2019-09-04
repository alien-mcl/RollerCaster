using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_invoking_a_custom_implementation
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "This is a test subject only.")]
        public static int Property(ISomeInterface instance)
        {
            return 14;
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "This is a test subject only.")]
        public static int Func(ISomeInterface instance)
        {
            return 13;
        }

        [Test]
        public void Should_call_provided_method_implementation()
        {
            new MulticastObject().ActLike<ISomeInterface>().Func().Should().Be(13);
        }

        [Test]
        public void Should_call_provided_property_implementation()
        {
            new MulticastObject().ActLike<ISomeInterface>().Property.Should().Be(14);
        }

        [OneTimeSetUp]
        public void Initialize()
        {
            MulticastObject.ImplementationOf<ISomeInterface>()
                .ForFunction(_ => _.Func()).ImplementedBy(_ => Func(_))
                .ForProperty(_ => _.Property).ImplementedBy(_ => Property(_));
        }
    }
}
