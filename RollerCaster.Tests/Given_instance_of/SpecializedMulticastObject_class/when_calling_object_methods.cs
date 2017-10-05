using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.SpecializedMulticastObject_class
{
    [TestFixture]
    public class when_calling_object_methods : SpecializedMulticastObjectTest
    {
        private const string ExpectedId = "Id";

        [Test]
        public void Should_call_underlying_GetHashCode_implementation()
        {
            MulticastObject.ActLike<IProduct>().GetHashCode().Should().Be(ExpectedId.GetHashCode());
        }

        [Test]
        public void Should_call_underlying_Equals_implementation()
        {
            MulticastObject.ActLike<IProduct>().Equals(MulticastObject.ActLike<IProduct>()).Should().BeTrue();
        }

        protected override void ScenarioSetup()
        {
            MulticastObject.Id = ExpectedId;
        }
    }
}
