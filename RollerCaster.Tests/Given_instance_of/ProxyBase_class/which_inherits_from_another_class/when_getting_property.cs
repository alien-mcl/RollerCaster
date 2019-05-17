using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.ProxyBase_class.which_inherits_from_another_class
{
    [TestFixture]
    public class when_getting_property
    {
        private Service Proxy { get; set; }

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

        [SetUp]
        public void Setup()
        {
            Proxy = new MulticastObject().ActLike<Service>();
            Proxy.Name = "Service name";
        }
    }
}
