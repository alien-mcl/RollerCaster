using System;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Collections;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class.when_setting_a_property
{
    [TestFixture]
    public class which_is_a_read_only_collection : ScenarioTest<SpecializedService, ReadOnlyCollection>
    {
        private static readonly ReadOnlyCollection Value = new ReadOnlyCollection(Array.Empty<string>());

        protected override string ExpectedPropertyName { get { return "YetAnotherValues"; } }

        protected override ReadOnlyCollection ExpectedValue { get { return Value; } }

        [Test]
        public void Should_set_that_property_correctly()
        {
            MulticastObject.Properties.Should().HavePropertySet<SpecializedService>(ExpectedPropertyName, Value);
        }
    }
}
