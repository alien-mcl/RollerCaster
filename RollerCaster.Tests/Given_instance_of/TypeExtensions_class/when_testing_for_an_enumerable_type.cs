using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Reflection;

namespace Given_instance_of.TypeExtensions_class
{
    [TestFixture]
    public class when_testing_for_an_enumerable_type
    {
        [Test]
        public void Should_throw_when_no_type_is_given()
        {
            ((Type)null).Invoking(type => type.IsAnEnumerable()).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_not_consider_string_as_an_enumerable()
        {
            typeof(string).IsAnEnumerable().Should().BeFalse();
        }

        [Test]
        public void Should_not_consider_an_array_of_bytes_as_an_enumerable()
        {
            typeof(byte[]).IsAnEnumerable().Should().BeFalse();
        }

        [Test]
        public void Should_consider_a_list_as_an_enumerable()
        {
            typeof(List<int>).IsAnEnumerable().Should().BeTrue();
        }

        [Test]
        public void Should_consider_an_array_list_as_an_enumerable()
        {
            typeof(ArrayList).IsAnEnumerable().Should().BeTrue();
        }

        [Test]
        public void Should_consider_a_dictionary_interface_as_an_enumerable()
        {
            typeof(IDictionary<string, string>).IsAnEnumerable().Should().BeTrue();
        }

        [Test]
        public void Should_consider_a_hash_set_as_an_enumerable()
        {
            typeof(HashSet<int>).IsAnEnumerable().Should().BeTrue();
        }
    }
}