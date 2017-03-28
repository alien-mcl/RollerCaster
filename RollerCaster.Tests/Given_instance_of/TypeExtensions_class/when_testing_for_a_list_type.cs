using System;
using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Reflection;

namespace Given_instance_of.TypeExtensions_class
{
    [TestFixture]
    public class when_testing_for_a_list_type
    {
        [Test]
        public void Should_throw_when_no_type_is_given()
        {
            ((Type)null).Invoking(type => type.IsAList()).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_not_consider_an_array_a_list()
        {
            typeof(byte[]).IsAList().Should().BeFalse();
        }

        [Test]
        public void Should_consider_an_array_list_as_a_list()
        {
            typeof(ArrayList).IsAList().Should().BeTrue();
        }

        [Test]
        public void Should_consider_a_generic_list_as_a_list()
        {
            typeof(List<int>).IsAList().Should().BeTrue();
        }
    }
}