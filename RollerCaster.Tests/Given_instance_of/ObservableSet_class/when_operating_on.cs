using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Collections;

namespace Given_instance_of.ObservableSet_class
{
    [TestFixture]
    public class when_operating_on
    {
        private ObservableSet<string> Set { get; set; }

        private ICollection<string> NewItems { get; set; }

        private ICollection<string> OldItems { get; set; }

        private ICollection<string> ClearedItems { get; set; }

        private string[] BoxedCopy { get; } = new string[1];

        private bool HasObservers { get; set; }

        public void TheTest()
        {
            Set.Add("2");
            Set.Remove("2");
            Set.Add("W");
            Set.Clear();
            HasObservers = Set.HasEventHandlerAdded;
            Set.ClearCollectionChanged();
            ((ICollection<string>)Set).Add("0");
            Set.CopyTo(BoxedCopy, 0);
        }

        [Test]
        public void Should_provide_correct_number_of_items()
        {
            Set.Count.Should().Be(1);
        }

        [Test]
        public void Should_provide_information_about_added_observers()
        {
            HasObservers.Should().BeTrue();
        }

        [Test]
        public void Should_provide_information_whether_list_is_read_only()
        {
            Set.IsReadOnly.Should().BeFalse();
        }

        [Test]
        public void Should_notify_about_new_item()
        {
            NewItems.Should().BeEquivalentTo("2", "W");
        }

        [Test]
        public void Should_notify_about_removed_item()
        {
            OldItems.Should().BeEquivalentTo("2");
        }

        [Test]
        public void Should_notify_about_cleared_item()
        {
            ClearedItems.Should().BeEquivalentTo("1", "W");
        }

        [Test]
        public void Should_contain_a_given_item()
        {
            Set.Contains("0").Should().BeTrue();
        }

        [Test]
        public void Should_copy_boxed_items()
        {
            BoxedCopy.Should().HaveCount(1).And.Subject.First().Should().Be("0");
        }

        [Test]
        public void Should_iterate_through_all_items()
        {
            foreach (string value in (IEnumerable)Set)
            {
                value.Should().Be("0");
            }
        }

        [Test]
        public void Should_not_remove_item_that_does_not_exist()
        {
            Set.Remove("Z").Should().BeFalse();
        }

        [SetUp]
        public void Setup()
        {
            NewItems = new List<string>();
            OldItems = new List<string>();
            ClearedItems = new List<string>();
            Set = new ObservableSet<string>();
            Set.Add("1");
            ICollection<string> oldItems = null;
            Set.CollectionChanged += (sender, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (string value in e.NewItems)
                        {
                            NewItems.Add(value);
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (string value in e.OldItems)
                        {
                            OldItems.Add(value);
                        }

                        if (oldItems != null)
                        {
                            OldItems = oldItems;
                        }

                        break;
                    case NotifyCollectionChangedAction.Reset:
                        oldItems = OldItems;
                        OldItems = ClearedItems;
                        break;
                }
            };
            TheTest();
        }
    }
}
