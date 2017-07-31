using System;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class.when_getting_a_property.which_is_a_reference_type
{
    [TestFixture]
    public class by_name : MulticastObjectTest
    {
        private const string ExpectedValue = "Product name";

        [Test]
        public void Should_obtain_value_of_that_property()
        {
            MulticastObject.GetProperty(typeof(IProduct), "Name").Should().Be(ExpectedValue);
        }

        [Test]
        public void Should_obtain_a_default_value_if_nothing_was_set()
        {
            new MulticastObject().GetProperty(typeof(IProduct), "Name").Should().BeNull();
        }

        [Test]
        public void Should_obtain_a_default_value_if_that_value_was_set()
        {
            MulticastObjectWithDefaultPropertyValueOf<IProduct>("Name").GetProperty(typeof(IProduct), "Name").Should().BeNull();
        }

        protected override void ScenarioSetup()
        {
            MulticastObject.SetProperty(typeof(IProduct), "Name", ExpectedValue);
        }

        private MulticastObject MulticastObjectWithDefaultPropertyValueOf<T>(string propertyName)
        {
            var propertyType = typeof(T).GetTypeInfo().GetDeclaredProperty(propertyName).PropertyType;
            var result = new MulticastObject();
            result.SetProperty(typeof(T), propertyName, propertyType.GetTypeInfo().IsValueType ? Activator.CreateInstance(propertyType) : null);
            return result;
        }
    }
}
