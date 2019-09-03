using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.ProxyBase_class.when_modifying
{
    [TestFixture]
    public class through_wired_property
    {
        private SomeSuperDuperSpecializedEntity Entity { get; set; }

        [Test]
        public void Should_provide_same_strongly_typed_instance()
        {
            Entity.Unwrap().ActLike<SomeSuperDuperSpecializedEntity>().Should().Be(Entity);
        }

        [Test]
        public void Should_set_id()
        {
            Entity.Id.Should().Be(13);
        }

        [Test]
        public void Should_set_related_id()
        {
            Entity.RelatedId.Should().Be(14);
        }

        [Test]
        public void Should_obtain_uniqueId()
        {
            Entity.UniqueId.Should().Be("13_14");
        }

        [Test]
        public void Should_provide_all_property_values()
        {
            Entity.Unwrap().PropertyValues.Select(_ => new { Property = _.Property.Name, _.Value })
                .ShouldBeEquivalentTo(new[]
                {
                    new { Property = "UniqueId", Value = (object)"13_14" },
                    new { Property = "RelatedId", Value = (object)14 },
                    new { Property = "Id", Value = (object)13 }
                });
        }

        [SetUp]
        public void Setup()
        {
            Entity = new MulticastObject().ActLike<SomeSuperDuperSpecializedEntity>();
            Entity.UniqueId = "13_14";
        }
    }
}
