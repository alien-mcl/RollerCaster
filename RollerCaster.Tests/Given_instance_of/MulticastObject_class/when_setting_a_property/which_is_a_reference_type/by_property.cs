using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class.when_setting_a_property.which_is_a_reference_type
{
    [TestFixture]
    public class by_property : ScenarioTest<IProduct, string>
    {
        protected override string ExpectedPropertyName { get { return "Name"; } }

        protected override string ExpectedValue { get { return "test"; } }

        public override void TheTest()
        {
            MulticastObject.SetProperty(typeof(IProduct).GetProperty(ExpectedPropertyName), ExpectedValue);
        }

        [Test]
        public void Should_set_that_property_correctly()
        {
            MulticastObject.Properties.Should().HavePropertySet<IProduct>(ExpectedPropertyName, ExpectedValue);
        }
    }
}
