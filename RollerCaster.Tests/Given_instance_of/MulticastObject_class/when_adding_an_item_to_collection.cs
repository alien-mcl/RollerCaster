using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_adding_an_item_to_collection : MulticastObjectTest
    {
        private const string ExpectedProperty = "Categories";

        public override void TheTest()
        {
            MulticastObject.SetProperty(typeof(IProduct).GetTypeInfo().GetProperty(ExpectedProperty), "test");
        }

        [Test]
        public void Should_append_that_item_to_collection()
        {
            MulticastObject.Properties.Should().HaveCollectionPropertyValues<IProduct, string>(ExpectedProperty, "first", "test");
        }

        protected override void ScenarioSetup()
        {
            MulticastObject.SetProperty(typeof(IProduct).GetTypeInfo().GetProperty(ExpectedProperty), new[] { "first" });
        }
    }
}
