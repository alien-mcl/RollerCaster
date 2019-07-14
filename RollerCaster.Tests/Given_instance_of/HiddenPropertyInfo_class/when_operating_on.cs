using System;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;
using RollerCaster.Reflection;

namespace Given_instance_of.HiddenPropertyInfo_class
{
    [TestFixture]
    public class when_operating_on
    {
        private MulticastObject Instance { get; set; }

        private HiddenPropertyInfo Property { get; set; }

        [Test]
        public void Should_obtain_property_name()
        {
            Property.Name.Should().Be("_Test");
        }

        [Test]
        public void Should_obtain_property_type()
        {
            Property.PropertyType.Should().Be(typeof(string));
        }

        [Test]
        public void Should_obtain_information_whether_the_property_can_be_assigned_to()
        {
            Property.CanWrite.Should().BeTrue();
        }

        [Test]
        public void Should_obtain_information_whether_the_property_can_be_read_from()
        {
            Property.CanRead.Should().BeTrue();
        }

        [Test]
        public void Should_obtain_attributes()
        {
            Property.Attributes.Should().Be(PropertyAttributes.None);
        }

        [Test]
        public void Should_match_declaring_and_reflected_types()
        {
            Property.DeclaringType.Should().Be(Property.ReflectedType);
        }
        
        [Test]
        public void Should_obtain_accessors()
        {
            Property.GetAccessors().Should().HaveCount(2);
        }

        [Test]
        public void Should_get_custom_attributes()
        {
            Property.GetCustomAttributes(false).Should().BeEmpty();
        }

        [Test]
        public void Should_obtain_value_trough_accessor()
        {
            Property.GetGetMethod().Invoke(Property, new object[] { Instance }).Should().Be("Test");
        }
        
        [Test]
        public void Should_obtain_index_parameters()
        {
            Property.GetIndexParameters().Should().BeEmpty();
        }

        [Test]
        public void Should_provide_information_whether_attribute_is_defined()
        {
            Property.IsDefined(typeof(Attribute), false).Should().BeFalse();
        }

        [Test]
        public void Should_set_value()
        {
            Property.SetValue(Instance, "2");
            Property.GetValue(Instance).Should().Be("2");
        }

        [SetUp]
        public void Setup()
        {
            Property = new HiddenPropertyInfo("Test", typeof(string), typeof(ITestResource));
            Instance = new MulticastObject();
            Property.GetSetMethod().Invoke(Property, new object[] { Instance, "Test" });
        }
    }
}
