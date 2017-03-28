using System;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class.when_getting_a_property
{
    [TestFixture]
    public class and_something_is_missing : MulticastObjectTest
    {
        [Test]
        public void Should_throw_when_no_object_type_is_given()
        {
            MulticastObject.Invoking(instance => instance.GetProperty(null, null))
                .ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("objectType");
        }

        [Test]
        public void Should_throw_when_no_property_name_is_given()
        {
            MulticastObject.Invoking(instance => instance.GetProperty(typeof(IProduct), null))
                .ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("propertyName");
        }

        [Test]
        public void Should_throw_when_empty_property_name_is_given()
        {
            MulticastObject.Invoking(instance => instance.GetProperty<IProduct, string>(String.Empty))
                .ShouldThrow<ArgumentOutOfRangeException>().And.ParamName.Should().Be("propertyName");
        }

        [Test]
        public void Should_throw_when_no_multicast_object_is_given()
        {
            ((MulticastObject)null).Invoking(instance => instance.GetProperty<IProduct, string>(null))
                .ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("multicastObject");
        }
    }
}
