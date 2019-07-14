using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Reflection;

namespace Given_instance_of.TypeExtensions_class
{
    [TestFixture]
    public class when_checking_whether_type_can_accept_null_value
    {
        [Test]
        public void Should_confirm_it_for_reference_type()
        {
            typeof(string).CanHaveNullValue().Should().BeTrue();
        }

        [Test]
        public void Should_confirm_it_for_nullable_type()
        {
            typeof(int?).CanHaveNullValue().Should().BeTrue();
        }
        
        [Test]
        public void Should_not_confirm_it_for_value_type()
        {
            typeof(int).CanHaveNullValue().Should().BeFalse();
        }
    }
}
