using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Collections;
using RollerCaster.Reflection;

namespace Given_instance_of.TypeExtensions_class
{
    [TestFixture]
    public class when_creating_a_default_value
    {
        [Test]
        public void Should_throw_when_no_type_is_given()
        {
            ((Type)null).GetDefaultValue().Should().BeNull();
        }

        [Test]
        public void Should_create_a_list_when_a_collection_is_needed()
        {
            typeof(ICollection<int>).GetDefaultValue(CollectionOptions.None).Should().BeOfType<List<int>>();
        }

        [Test]
        public void Should_create_an_observable_list_when_a_concurrent_collection_is_needed()
        {
            typeof(ICollection<int>).GetDefaultValue(CollectionOptions.Concurrent).Should().BeOfType<ObservableList<int>>();
        }

        [Test]
        public void Should_create_an_observable_list_when_an_observable_collection_is_needed()
        {
            typeof(ICollection<int>).GetDefaultValue().Should().BeOfType<ObservableList<int>>();
        }

        [Test]
        public void Should_create_a_dictionary_when_a_dictionary_is_needed()
        {
            typeof(IDictionary<int, int>).GetDefaultValue(CollectionOptions.None).Should().BeOfType<Dictionary<int, int>>();
        }

        [Test]
        public void Should_create_a_concurrent_dictionary_when_a_concurrent_dictionary_is_needed()
        {
            typeof(IDictionary<int, int>).GetDefaultValue(CollectionOptions.Concurrent).Should().BeOfType<ConcurrentDictionary<int, int>>();
        }

        [Test]
        public void Should_create_a_hash_set_when_a_set_is_needed()
        {
            typeof(ISet<int>).GetDefaultValue().Should().BeOfType<HashSet<int>>();
        }

        [Test]
        public void Should_create_an_array_when_an_array_is_needed()
        {
            typeof(int[]).GetDefaultValue().Should().BeOfType<int[]>();
        }

        [Test]
        public void Should_create_a_specialized_collection_when_such_is_needed()
        {
            typeof(ReadOnlySpecializedCollection).GetDefaultValue().Should().BeOfType<ReadOnlySpecializedCollection>();
        }

        [Test]
        public void Should_create_a_default_value_when_value_type_is_needed()
        {
            typeof(int).GetDefaultValue().Should().Be(default(int));
        }

        [Test]
        public void Should_return_null_if_other_reference_type_is_needed()
        {
            typeof(string).GetDefaultValue().Should().BeNull();
        }
    }
}
