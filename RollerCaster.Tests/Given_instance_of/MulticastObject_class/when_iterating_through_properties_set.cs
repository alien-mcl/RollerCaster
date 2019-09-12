using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Collections;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_iterating_through_properties_set : MulticastObjectTest
    {
        private static readonly IEnumerable<MulticastPropertyValue> ExpectedProperties = new[]
        {
            new MulticastPropertyValue(typeof(IProduct), typeof(IProduct).GetProperty("Name"), "Name"),
            new MulticastPropertyValue(typeof(IProduct), typeof(IProduct).GetProperty("Ordinal"), 1),
            new MulticastPropertyValue(typeof(IProduct), typeof(IProduct).GetProperty("Price"), 3.14159d),
            new MulticastPropertyValue(typeof(IProduct), typeof(IProduct).GetProperty("CreatedOn"), default(DateTime)),
            new MulticastPropertyValue(typeof(IProduct), typeof(IProduct).GetProperty("Categories"), new ObservableList<string>()),
            new MulticastPropertyValue(typeof(IProduct), typeof(IProduct).GetProperty("Properties"), new ConcurrentDictionary<string, string>()),
            new MulticastPropertyValue(typeof(IProduct), typeof(IProduct).GetProperty("Keywords"), new ReadOnlySpecializedCollection(Array.Empty<string>())),
            new MulticastPropertyValue(typeof(IThing), typeof(IThing).GetProperty("Description"), "Description")
        };

        private IEnumerable<MulticastPropertyValue> Result { get; set; }

        public override void TheTest()
        {
            Result = MulticastObject.PropertyValues.ToArray();
        }

        [Test]
        public void Should_enumerate_through_all_properties_set_correctly()
        {
            Result.Select(_ => $"{_.CastedType.FullName}.{_.Property.Name}")
                .Should().BeEquivalentTo(ExpectedProperties.Select(_ => $"{_.CastedType.FullName}.{_.Property.Name}"));
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
            var product = MulticastObject.ActLike<IProduct>();
            product.Name = "Name";
            product.Ordinal = 1;
            product.Price = 3.14159;
            var thing = MulticastObject.ActLike<IThing>();
            thing.Description = "Description";
        }
    }
}
