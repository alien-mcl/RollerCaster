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
            ((MulticastObject)null).Invoking(instance => instance.SetProperty<IProduct, string>(null, null))
                .ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("multicastObject");
        }

        [Test]
        public void Should_throw_when_null_is_given_instead_of_the_multicast_object()
        {
            ((MulticastObject)null).Invoking(instance => instance.SetProperty<string>(typeof(IProduct), null, null))
                .ShouldThrow<ArgumentNullException>().And.ParamName.Should().Be("multicastObject");
        }
    }
}
