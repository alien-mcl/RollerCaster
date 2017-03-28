using System;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastPropertyValue_class
{
    [TestFixture]
    public class when_comparing
    {
        private static readonly Type CastedType = typeof(IProduct);
        private static readonly PropertyInfo Property = CastedType.GetProperty("Name");

        private MulticastPropertyValue PropertyValue { get; set; }

        [Test]
        public void Should_consider_as_equal_same_instances()
        {
            (PropertyValue = new MulticastPropertyValue(typeof(IProduct), Property, "Test")).Equals(PropertyValue).Should().BeTrue();
        }

        [Test]
        public void Should_consider_as_equal_same_settings()
        {
            new MulticastPropertyValue(typeof(IProduct), Property, "Test").Equals(new MulticastPropertyValue(typeof(IProduct), Property, "Test")).Should().BeTrue();
        }

        [Test]
        public void Should_calculate_hash_code_correctly()
        {
            new MulticastPropertyValue(typeof(IProduct), Property, "Test").GetHashCode().Should().Be(CastedType.GetHashCode() ^ Property.GetHashCode() ^ "Test".GetHashCode());
        }
    }
}
