using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class.when_setting_a_property
{
    [TestFixture]
    public class which_is_a_reference_type_with_null_value : ScenarioTest<IProduct, string>
    {
        protected override string ExpectedPropertyName { get { return "Name"; } }

        protected override string ExpectedValue { get { return null; } }

        [Test]
        public void Should_set_that_property_correctly()
        {
            MulticastObject.Properties.Should().NotHavePropertySet<IProduct>(ExpectedPropertyName);
        }
    }
}
