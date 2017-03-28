using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class.when_setting_a_property
{
    [TestFixture]
    public class which_is_a_value_type_with_default_value : ScenarioTest<IProduct, int>
    {
        protected override string ExpectedPropertyName { get { return "Ordinal"; } }

        protected override int ExpectedValue { get { return 0; } }

        [Test]
        public void Should_set_that_property_correctly()
        {
            MulticastObject.Properties.Should().HavePropertySet<IProduct, int>(ExpectedPropertyName, ExpectedValue);
        }
    }
}
