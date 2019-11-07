using System;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Data;
using RollerCaster.Reflection;

namespace Given_instance_of.TypeExtensions_class
{
    [TestFixture]
    public class when_searching_for_property
    {
        [Test]
        public void Should_throw_when_no_type_is_given()
        {
            ((Type)null).Invoking(type => type.FindProperty(null))
                .Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("type");
        }

        [Test]
        public void Should_throw_when_no_property_name_is_given()
        {
            typeof(string).Invoking(instance => instance.FindProperty(null))
                .Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("name");
        }

        [Test]
        public void Should_throw_when_property_name_given_is_empty()
        {
            typeof(string).Invoking(instance => instance.FindProperty(String.Empty))
                .Should().Throw<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("name");
        }

        [Test]
        public void Should_return_direct_property()
        {
            typeof(IProduct).FindProperty("Price").Name.Should().Be("Price");
        }

        [Test]
        public void Should_return_an_inherited_property()
        {
            typeof(ISpecializedProduct).FindProperty("Image").Name.Should().Be("Image");
        }
    }
}