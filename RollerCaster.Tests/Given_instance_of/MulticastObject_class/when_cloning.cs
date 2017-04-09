using System;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_cloning : MulticastObjectTest
    {
        private ISpecializedProduct Expected { get; set; }

        private ISpecializedProduct Result { get; set; }

        public override void TheTest()
        {
            Result = Expected.Unwrap().Clone(true).ActLike<ISpecializedProduct>();
        }

        [Test]
        public void Should_copy_a_name()
        {
            Result.Name.Should().Be(Expected.Name);
        }

        [Test]
        public void Should_copy_a_creation_date()
        {
            Result.CreatedOn.Should().Be(Expected.CreatedOn);
        }

        [Test]
        public void Should_copy_multicast_object_instance()
        {
            Result.Related.Name.Should().Be(Expected.Related.Name);
        }

        [Test]
        public void Should_prevent_loop_while_cloning()
        {
            ((ISpecializedProduct)Result.Related).Related.Unwrap().Should().Be(Result.Unwrap());
        }

        protected override void ScenarioSetup()
        {
            Expected = MulticastObject.ActLike<ISpecializedProduct>();
            Expected.Name = "Test product name";
            Expected.CreatedOn = new DateTime(2017, 04, 09, 19, 50, 00);
            Expected.Related = new MulticastObject().ActLike<ISpecializedProduct>();
            Expected.Related.Name = "Related product";
            ((ISpecializedProduct)Expected.Related).Related = Expected;
        }
    }
}
