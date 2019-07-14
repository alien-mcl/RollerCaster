using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Data;
using RollerCaster.Reflection;

namespace Given_instance_of.PropertyInfoEqualityComparer_class
{
    [TestFixture]
    public class when_operating_on
    {
        private static readonly HiddenPropertyInfo Left = new HiddenPropertyInfo("Test", typeof(string), typeof(ITestResource));

        private static readonly HiddenPropertyInfo Right = new HiddenPropertyInfo("Test", typeof(string), typeof(ITestResource));

        [Test]
        public void Should_confirm_equality_of_nulls()
        {
            PropertyInfoEqualityComparer.Default.Equals(null, null).Should().BeTrue();
        }

        [Test]
        public void Should_confirm_equality_of_same_instances()
        {
            PropertyInfoEqualityComparer.Default.Equals(Left, Left).Should().BeTrue();
        }
        
        [Test]
        public void Should_confirm_equality_of_same_properties()
        {
            PropertyInfoEqualityComparer.Default.Equals(Left, Right).Should().BeTrue();
        }

        [Test]
        public void Should_generate_correct_hash_code()
        {
            PropertyInfoEqualityComparer.Default.GetHashCode(Left)
                .Should().Be(Left.PropertyType.GetHashCode() ^ Left.Name.GetHashCode() ^ Left.DeclaringType.GetHashCode());
        }
    }
}
