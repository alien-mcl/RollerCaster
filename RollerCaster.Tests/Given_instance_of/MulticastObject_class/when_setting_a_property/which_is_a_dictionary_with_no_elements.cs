using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class.when_setting_a_property
{
    [TestFixture]
    public class which_is_a_dictionary_with_no_elements : ScenarioTest<IProduct, IDictionary<string, string>>
    {
        protected override string ExpectedPropertyName { get { return "Properties"; } }

        protected override IDictionary<string, string> ExpectedValue { get { return new Dictionary<string, string>(); } }

        [Test]
        public void Should_set_that_property_correctly()
        {
            MulticastObject.Properties.Should().HaveDictionaryPropertySet<IProduct>(ExpectedPropertyName, ExpectedValue);
        }
    }
}
