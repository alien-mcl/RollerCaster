using FluentAssertions;
using NUnit.Framework;

namespace Given_instance_of.ObservableSet_class.when_working_with
{
    [TestFixture]
    public class without_observing : ScenarioTest
    {
        [Test]
        public void Should_acknowledge_set_to_be_subset_of_set()
        {
            Set.IsSubsetOf(Set).Should().BeTrue();
        }

        [Test]
        public void Should_acknowledge_set_to_be_superset_of_set()
        {
            Set.IsSupersetOf(Set).Should().BeTrue();
        }

        [Test]
        public void Should_acknowledge_set_to_be_proper_subset_of_primes()
        {
            Set.IsProperSubsetOf(Primes).Should().BeTrue();
        }

        [Test]
        public void Should_acknowledge_primes_to_be_proper_superset_of_set()
        {
            Primes.IsProperSupersetOf(Set).Should().BeTrue();
        }

        [Test]
        public void Should_acknowledge_set_to_overlap_with_three_first_numbers_set()
        {
            Set.Overlaps(FirstThree).Should().BeTrue();
        }
        
        [Test]
        public void Should_confirm_set_is_equal_to_set()
        {
            Set.SetEquals(Set).Should().BeTrue();
        }

        [Test]
        public void Should_intersect_set_with_first_three_numbers()
        {
            After(() => Set.IntersectWith(FirstThree)).ShouldBeEquivalentTo(new[] { 1, 2 });
        }
        
        [Test]
        public void Should_leave_set_except_first_three_numbers()
        {
            After(() => Set.ExceptWith(FirstThree)).ShouldBeEquivalentTo(new[] { 3 });
        }
        
        [Test]
        public void Should_leave_first_three_numbers_symmetrically_except_set()
        {
            After(() => Set.SymmetricExceptWith(FirstThree)).ShouldBeEquivalentTo(new[] { 0, 3 });
        }
        
        [Test]
        public void Should_union_first_three_numbers_with_set()
        {
            After(() => Set.UnionWith(FirstThree)).ShouldBeEquivalentTo(new[] { 0, 1, 2, 3 });
        }
    }
}
