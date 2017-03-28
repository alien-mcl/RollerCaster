using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_undoing_the_cast : MulticastObjectTest
    {
        private IProduct Product { get; set; }

        public override void TheTest()
        {
            Product.UndoActLike<IProduct>();
        }

        [Test]
        public void Should_retract_the_typecast()
        {
            MulticastObject.CastedTypes.Should().NotContain(typeof(IProduct));
        }

        protected override void ScenarioSetup()
        {
            Product = MulticastObject.ActLike<IProduct>();
        }
    }
}
