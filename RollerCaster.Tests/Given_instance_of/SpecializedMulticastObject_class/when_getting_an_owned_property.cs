using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Data;

namespace Given_instance_of.SpecializedMulticastObject_class
{
    [TestFixture]
    public class when_getting_an_owned_property : SpecializedMulticastObjectTest
    {
        private const string ExpectedId = "Id";

        private object Result { get; set; }

        public override void TheTest()
        {
            Result = MulticastObject.GetProperty(typeof(SpecializedMulticastObject).GetTypeInfo().GetProperty("Id"));
        }

        [Test]
        public void Should_obtain_a_value_directly()
        {
            Result.Should().Be(ExpectedId);
        }

        protected override void ScenarioSetup()
        {
            MulticastObject.SetProperty(typeof(SpecializedMulticastObject).GetTypeInfo().GetProperty("Id"), ExpectedId);
        }
    }
}
