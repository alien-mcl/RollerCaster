using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.ProxyBase_class
{
    [TestFixture]
    public class when_getting_property
    {
        private IProduct Proxy { get; set; }

        [Test]
        public void Should_set_product_name()
        {
            Proxy.Name.Should().Be("Product name");
        }

        [Test]
        public void Should_set_product_price()
        {
            Proxy.Price.Should().Be(12.3);
        }

        [Test]
        public void Should_add_product_category()
        {
            Proxy.Categories.Should().BeEquivalentTo("Category 1");
        }

        [Test]
        public void Should_set_product_property()
        {
            Proxy.Properties.Should().ContainKey("key").WhichValue.Should().Be("value");
        }

        [SetUp]
        public void Setup()
        {
            Proxy = new MulticastObject().ActLike<IProduct>();
            Proxy.Name = "Product name";
            Proxy.Price = 12.3;
            Proxy.Categories.Add("Category 1");
            Proxy.Properties["key"] = "value";
        }
    }
}
