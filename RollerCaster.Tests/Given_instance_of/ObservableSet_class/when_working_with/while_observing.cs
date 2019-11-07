using System;
using FluentAssertions;
using NUnit.Framework;

namespace Given_instance_of.ObservableSet_class.when_working_with
{
    [TestFixture]
    public class while_observing : ScenarioTest
    {
        [Test]
        public void Should_throw_when_no_other_collection_is_given_for_intersection()
        {
            Set.Invoking(_ => _.IntersectWith(null))
                .Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("other");
        }

        [Test]
        public void Should_throw_when_no_other_collection_is_given_for_union()
        {
            Set.Invoking(_ => _.UnionWith(null))
                .Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("other");
        }

        [Test]
        public void Should_throw_when_no_other_collection_is_given_for_exception()
        {
            Set.Invoking(_ => _.ExceptWith(null))
                .Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("other");
        }

        [Test]
        public void Should_throw_when_no_other_collection_is_given_for_symmetric_exception()
        {
            Set.Invoking(_ => _.SymmetricExceptWith(null))
                .Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("other");
        }

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
        public void Should_intersect_set_with_first_three_numbers()
        {
            After(() => Set.IntersectWith(FirstThree)).Should().BeEquivalentTo(1, 2);
        }
        
        [Test]
        public void Should_leave_set_except_first_three_numbers()
        {
            After(() => Set.ExceptWith(FirstThree)).Should().BeEquivalentTo(3);
        }
        
        [Test]
        public void Should_leave_first_three_numbers_symmetrically_except_set()
        {
            After(() => Set.SymmetricExceptWith(FirstThree)).Should().BeEquivalentTo(0, 3);
        }
        
        [Test]
        public void Should_union_first_three_numbers_with_set()
        {
            After(() => Set.UnionWith(FirstThree)).Should().BeEquivalentTo(0, 1, 2, 3);
        }

        protected override void ScenarioSetup()
        {
            Set.CollectionChanged += (sender, e) => { };
            Primes.CollectionChanged += (sender, e) => { };
        }
    }
}
