using System;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class.when_setting_a_property
{
    [TestFixture]
    public class which_is_a_value_type_property : ScenarioTest<IProduct, int>
    {
        protected override string ExpectedPropertyName { get { return "Ordinal"; } }

        protected override int ExpectedValue { get { return 1; } }

        [Test]
        public void Should_set_that_property_correctly()
        {
            MulticastObject.Properties.Should().HavePropertySet<IProduct, int>(ExpectedPropertyName, ExpectedValue);
        }

        [Test]
        public void Should_throw_when_no_multicast_object_is_given()
        {
            ((MulticastObject)null).Invoking(instance => instance.SetProperty<IProduct, string>("Name", null))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("multicastObject");
        }

        [Test]
        public void Should_throw_when_no_property_name_is_given()
        {
            ((MulticastObject)null).Invoking(instance => instance.SetProperty<IProduct, string>(null, null))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("propertyName");
        }

        [Test]
        public void Should_throw_when_empty_property_name_is_given()
        {
            ((MulticastObject)null).Invoking(instance => instance.SetProperty<IProduct, string>(String.Empty, null))
                .Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("propertyName");
        }
    }
}
