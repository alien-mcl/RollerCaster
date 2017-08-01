using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_working_as_dynamic : MulticastObjectTest
    {
        private string ExpectedName { get; set; }

        private dynamic Dynamic { get; set; }

        [Test]
        public void Should_get_the_dynamic_property()
        {
            ((object)Dynamic.Name).Should().Be(ExpectedName);
        }

        [Test]
        public void Should_set_the_dynamic_property()
        {
            ((object)(Dynamic.Name = ExpectedName = "New product name")).Should().Be(ExpectedName);
        }

        protected override void ScenarioSetup()
        {
            MulticastObject.SetProperty(typeof(IProduct).GetTypeInfo().GetProperty("Name"), ExpectedName = "Product name");
            Dynamic = MulticastObject;
        }
    }
}
