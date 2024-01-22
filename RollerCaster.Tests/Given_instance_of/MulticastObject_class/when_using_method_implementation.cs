using FluentAssertions;
using NUnit.Framework;
using RollerCaster;
using RollerCaster.Data;

namespace Given_instance_of.MulticastObject_class
{
    [TestFixture]
    public class when_using_method_implementation
    {
        [Test]
        public void Should_not_throw()
        {
            new MulticastObject().Invoking(_ => _.ActLike<ImplementingType>())
                .Should().NotThrow();
        }
    }
}
