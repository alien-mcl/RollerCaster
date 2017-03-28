using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Reflection;

namespace Given_instance_of.TypeExtensions_class
{
    [TestFixture]
    public class when_obtaining_an_item_type
    {
        [Test]
        public void Should_throw_when_no_type_is_given()
        {
            ((Type)null).Invoking(type => type.GetItemType()).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_return_an_array_element_type()
        {
            typeof(byte[]).GetItemType().Should().Be(typeof(byte));
        }

        [Test]
        public void Should_return_a_generic_item_type_for_generic_list()
        {
            typeof(List<int>).GetItemType().Should().Be(typeof(int));
        }

        [Test]
        public void Should_return_self_for_non_collective_types()
        {
            typeof(int).GetItemType().Should().Be(typeof(int));
        }
    }
}