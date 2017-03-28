using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_iterating_through_properties_set : MulticastObjectTest
    {
        private static readonly IEnumerable<MulticastPropertyValue> ExpectedProperties = new[]
        {
            new MulticastPropertyValue(typeof(IProduct), typeof(IProduct).GetTypeInfo().GetProperty("Name"), "Name"),
            new MulticastPropertyValue(typeof(IProduct), typeof(IProduct).GetTypeInfo().GetProperty("Ordinal"), 1),
            new MulticastPropertyValue(typeof(IProduct), typeof(IProduct).GetTypeInfo().GetProperty("Price"), 3.14159d),
            new MulticastPropertyValue(typeof(IThing), typeof(IThing).GetTypeInfo().GetProperty("Description"), "Description")
        };

        private IEnumerable<MulticastPropertyValue> Result { get; set; }

        public override void TheTest()
        {
            Result = MulticastObject.PropertyValues.ToArray();
        }

        [Test]
        public void Should_enumerate_through_all_properties_set_correctly()
        {
            Result.Should().BeEquivalentTo(ExpectedProperties);
        }

        [Test]
        public void Should_configure_casted_type_of_a_value_correctly()
        {
            Result.First().CastedType.Should().Be(ExpectedProperties.First().CastedType);
        }

        [Test]
        public void Should_configure_property_of_a_value_correctly()
        {
            Result.First().Property.Should().BeSameAs(ExpectedProperties.First().Property);
        }

        [Test]
        public void Should_configure_a_value_correctly()
        {
            Result.First().Value.Should().Be(ExpectedProperties.First().Value);
        }

        protected override void ScenarioSetup()
        {
            foreach (var propertyValue in ExpectedProperties)
            {
                MulticastObject.SetProperty(propertyValue.Property.DeclaringType, propertyValue.Property.Name, propertyValue.Value);
            }
        }
    }
}
