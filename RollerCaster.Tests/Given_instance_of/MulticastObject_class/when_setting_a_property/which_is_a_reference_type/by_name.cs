﻿using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class.when_setting_a_property.which_is_a_reference_type
{
    [TestFixture]
    public class by_name : ScenarioTest<IProduct, string>
    {
        protected override string ExpectedPropertyName { get { return "Name"; } }

        protected override string ExpectedValue { get { return "test"; } }

        [Test]
        public void Should_set_that_property_correctly()
        {
            MulticastObject.Properties.Should().HavePropertySet<IProduct>(ExpectedPropertyName, ExpectedValue);
        }
    }
}
