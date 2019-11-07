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
        public void Should_throw_when_no_property_info_is_given()
        {
            MulticastObject.Invoking(instance => instance.GetProperty(null))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("propertyInfo");
        }

        [Test]
        public void Should_throw_when_no_property_name_is_given()
        {
            MulticastObject.Invoking(instance => instance.GetProperty<IProduct, string>(null))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("propertyName");
        }

        [Test]
        public void Should_throw_when_empty_property_name_is_given()
        {
            MulticastObject.Invoking(instance => instance.GetProperty<IProduct, string>(String.Empty))
                .Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("propertyName");
        }

        [Test]
        public void Should_throw_when_no_multicast_object_is_given()
        {
            ((MulticastObject)null).Invoking(instance => instance.GetProperty<IProduct, string>("Name"))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("multicastObject");
        }
    }
}
