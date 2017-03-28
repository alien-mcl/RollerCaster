using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.DynamicExtensions_class
{
    [TestFixture]
    public class when_object_is_given
    {
        private MulticastObject MulticastObject { get; set; }

        [Test]
        public void Should_throw_when_that_object_is_neither_a_MulticastObject_nor_IProxy()
        {
            new Object().Invoking(instance => instance.Unwrap()).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void Should_confirm_that_MulticastObject_is_an_IProduct()
        {
            new MulticastObject().ActLike(typeof(IProduct)).ActLike<IThing>().Is<IProduct>().Should().BeTrue();
        }

        [Test]
        public void Should_build_a_type_name_correctly()
        {
            typeof(IDictionary<string, ICollection<int[]>>).GetName()
                .Should().Be("System_Collections_Generic_IDictionaryOf_System_String_And_System_Collections_Generic_ICollectionOf_ArrayOf_System_Int32");
        }

        [Test]
        public void Should_throw_when_no_casted_type_is_given()
        {
            new MulticastObject().Invoking(instance => instance.ActLike(null)).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_unwrap_a_multicast_object()
        {
            (MulticastObject = new MulticastObject()).ActLike<IProduct>().Unwrap().Should().Be(MulticastObject);
        }

        [Test]
        public void Should_successfully_try_to_unwrap_a_multicast_object()
        {
            MulticastObject result;
            (MulticastObject = new MulticastObject()).ActLike<IProduct>().TryUnwrap(out result).Should().BeTrue();
        }

        [Test]
        public void Should_throw_when_unwrapping_other_than_MulticastObject_instance()
        {
            new Object().Invoking(instance => instance.Unwrap()).ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void Should_throw_when_the_casted_type_is_not_an_interface()
        {
            new MulticastObject().Invoking(instance => instance.ActLike(typeof(string))).ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}
