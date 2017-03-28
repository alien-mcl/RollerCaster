using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class.when_setting_a_property
{
    [TestFixture]
    public class which_is_a_collection : ScenarioTest<IProduct, ICollection<string>>
    {
        protected override string ExpectedPropertyName { get { return "Categories"; } }

        protected override ICollection<string> ExpectedValue { get { return new List<string>() { "test" }; } }

        [Test]
        public void Should_set_that_property_correctly()
        {
            MulticastObject.Properties.Should().HaveCollectionPropertySet<IProduct>(ExpectedPropertyName, ExpectedValue);
        }
    }
}
