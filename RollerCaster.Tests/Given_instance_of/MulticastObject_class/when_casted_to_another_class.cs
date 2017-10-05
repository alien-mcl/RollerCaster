using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Collections;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_casted_to_another_class : MulticastObjectTest
    {
        private const double ExpectedPrice = 3.14159;
        private const string ExpectedName = "test";
        private const string ExpectedCategory = "category";
        private const string ExpectedPropertyKey = "key";
        private const string ExpectedPropertyValue = "value";
        private const string ExpectedImage = "image";

        private SpecializedProduct Result { get; set; }

        public override void TheTest()
        {
            Result = MulticastObject.ActLike<SpecializedProduct>();
        }

        [Test]
        public void Should_cast_the_entity_correctly()
        {
            Result.Should().BeAssignableTo<SpecializedProduct>();
        }

        [Test]
        public void Should_set_a_value_type_property_correctly()
        {
            (Result.Price = ExpectedPrice).Should().Be(ExpectedPrice);
        }

        [Test]
        public void Should_set_a_reference_type_property_correctly()
        {
            (Result.Name = ExpectedName).Should().Be(ExpectedName);
        }

        [Test]
        public void Should_add_an_item_to_the_collection()
        {
            Result.Categories.AddNext(ExpectedCategory).Should().Contain(ExpectedCategory);
        }

        [Test]
        public void Should_add_an_item_to_the_dictionary()
        {
            Result.Properties.AddNext(ExpectedPropertyKey, ExpectedPropertyValue).Should().ContainKey(ExpectedPropertyKey).WhichValue.Should().Be(ExpectedPropertyValue);
        }

        [Test]
        public void Should_set_a_virtual_property()
        {
            (Result.Image = ExpectedImage).Should().Be(ExpectedImage);
        }

        [Test]
        public void Should_use_underlying_equals_for_equality_comparison()
        {
            Result.Equals(MulticastObject).Should().BeTrue();
        }

        [Test]
        public void Should_use_underlying_hash_code_for_equality_comparison()
        {
            Result.GetHashCode().Should().Be(MulticastObject.GetHashCode());
        }
    }
}
