using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_deep_cloning : MulticastObjectTest
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
            Result.Related.Related.Unwrap().Should().Be(Result.Unwrap());
        }

        [Test]
        public void Should_clone_dictionary_values_correctly()
        {
            Result.Similar.Should().HaveCount(1).And.Subject.First().Value.Name.Should().Be(Expected.Related.Name);
        }

        [Test]
        public void Should_clone_dictionary_keys_correctly()
        {
            Result.Similar.Should().HaveCount(1).And.Subject.First().Key.Should().Be("test");
        }

        [Test]
        public void Should_clone_collection_correctly()
        {
            Result.Connected.Should().HaveCount(1).And.Subject.First().Name.Should().Be(Expected.Related.Name);
        }

        protected override void ScenarioSetup()
        {
            Expected = MulticastObject.ActLike<ISpecializedProduct>();
            Expected.Name = "Test product name";
            Expected.CreatedOn = new DateTime(2017, 04, 09, 19, 50, 00);
            Expected.Related = new MulticastObject().ActLike<ISpecializedProduct>();
            Expected.Related.Name = "Related product";
            Expected.Connected.Add(Expected.Related);
            Expected.Similar.Add("test", Expected.Related);
            Expected.Related.Related = Expected;
        }
    }
}
