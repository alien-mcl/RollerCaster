using System;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.ProxyBase_class.when_modifying
{
    [TestFixture]
    public class when_proxy_is_locked
    {
        private SomeSuperDuperSpecializedEntity Entity { get; set; }

        [Test]
        public void Should_throw()
        {
            Entity.Invoking(_ => _.UniqueId = "test").Should().Throw<InvalidOperationException>()
                .Which.Message.Should().Be("This instance is locked.");
        }

        [SetUp]
        public void Setup()
        {
            var multicastObject = new MulticastObject();
            multicastObject.LockInstance();
            Entity = multicastObject.ActLike<SomeSuperDuperSpecializedEntity>();
        }
    }
}
