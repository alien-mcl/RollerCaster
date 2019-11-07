using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Reflection;

namespace Given_instance_of.TypeExtensions_class
{
    [TestFixture]
    public class when_testing_for_a_dictionary_type
    {
        [Test]
        public void Should_throw_when_no_type_is_given()
        {
            ((Type)null).Invoking(type => type.IsADictionary()).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Should_not_consider_string_as_a_dictionary()
        {
            typeof(string).IsADictionary().Should().BeFalse();
        }

        [Test]
        public void Should_not_consider_an_array_of_bytes_as_a_dictionary()
        {
            typeof(byte[]).IsADictionary().Should().BeFalse();
        }

        [Test]
        public void Should_not_consider_a_list_as_a_dictionary()
        {
            typeof(List<int>).IsADictionary().Should().BeFalse();
        }

        [Test]
        public void Should_not_consider_an_array_list_as_a_dictionary()
        {
            typeof(ArrayList).IsADictionary().Should().BeFalse();
        }

        [Test]
        public void Should_consider_a_generic_dictionary_interface_as_a_dictionary()
        {
            typeof(IDictionary<string, string>).IsADictionary().Should().BeTrue();
        }

        [Test]
        public void Should_consider_a_dictionary_interface_as_a_dictionary()
        {
            typeof(IDictionary).IsADictionary().Should().BeTrue();
        }

        [Test]
        public void Should_not_consider_a_hash_set_as_a_dictionary()
        {
            typeof(HashSet<int>).IsADictionary().Should().BeFalse();
        }
    }
}