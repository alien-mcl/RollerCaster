using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Data;

namespace Given_instance_of.SpecializedMulticastObject_class
{
    [TestFixture]
    public class when_setting_an_owned_property : SpecializedMulticastObjectTest
    {
        private const string ExpectedId = "Id";

        public override void TheTest()
        {
            MulticastObject.SetProperty(typeof(SpecializedMulticastObject).GetTypeInfo().GetProperty("Id"), ExpectedId);
        }

        [Test]
        public void Should_not_touch_the_multicast_properties()
        {
            MulticastObject.Properties.Should().BeEmpty();
        }

        [Test]
        public void Should_set_the_property_correctly()
        {
            MulticastObject.Id.Should().Be(ExpectedId);
        }
    }
}
