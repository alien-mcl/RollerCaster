using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.ProxyBase_class.which_inherits_from_another_class.and_inherits_even_more
{
    [TestFixture]
    public class when_getting_property
    {
        private SpecializedService Proxy { get; set; }

        [Test]
        public void Should_set_service_name()
        {
            Proxy.ServiceName.Should().Be("Service name");
        }

        [Test]
        public void Should_set_product_name()
        {
            Proxy.Name.Should().Be("Service name");
        }

        [Test]
        public void Should_set_ordered_indicator()
        {
            Proxy.IsOrdered.Should().BeTrue();
        }

        [Test]
        public void Should_set_some_value()
        {
            Proxy.SomeValue.Should().Be(10);
        }

        [Test]
        public void Should_get_another_value_serialized()
        {
            Proxy.AnotherValueSerialized.Should().Be("1");
        }

        [SetUp]
        public void Setup()
        {
            Proxy = new MulticastObject().ActLike<SpecializedService>();
            Proxy.Name = "Service name";
            Proxy.IsOrdered = true;
            Proxy.SomeValue = 10;
            Proxy.SomeValue = 5;
            Proxy.AnotherValue = 1;
        }
    }
}
