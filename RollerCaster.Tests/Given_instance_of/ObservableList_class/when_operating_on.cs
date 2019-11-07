using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Collections;

namespace Given_instance_of.ObservableList_class
{
    [TestFixture]
    public class when_operating_on
    {
        private ObservableList<string> List { get; set; }

        private ICollection<string> NewItems { get; set; }

        private IDictionary<string, string> ChangedItems { get; set; }

        private ICollection<string> OldItems { get; set; }

        private ICollection<string> ClearedItems { get; set; }

        private object[] UnboxedCopy { get; } = new object[1];

        private string[] BoxedCopy { get; } = new string[1];

        private string OnlyItem { get; set; }

        private bool HasObservers { get; set; }

        public void TheTest()
        {
            List.Add("2");
            List.Remove("2");
            ((IList)List)[0] = "X";
            OnlyItem = (string)((IList)List)[0];
            List.Clear();
            ((IList)List).Insert(0, "W");
            ((IList)List).Remove("W");
            List.Add("3");
            List.RemoveAt(List.IndexOf("3"));
            HasObservers = List.HasEventHandlerAdded;
            List.ClearCollectionChanged();
            ((IList)List).Add("0");
            List.CopyTo(BoxedCopy, 0);
            ((IList)List).CopyTo(UnboxedCopy, 0);
        }

        [Test]
        public void Should_provide_correct_number_of_items()
        {
            List.Count.Should().Be(1);
        }

        [Test]
        public void Should_provide_sync_root()
        {
            ((ICollection)List).SyncRoot.Should().NotBeNull();
        }

        [Test]
        public void Should_provide_information_whether_list_is_synchronized()
        {
            ((ICollection)List).IsSynchronized.Should().BeFalse();
        }

        [Test]
        public void Should_provide_information_about_added_observers()
        {
            HasObservers.Should().BeTrue();
        }

        [Test]
        public void Should_provide_information_whether_list_is_read_only()
        {
            ((ICollection<string>)List).IsReadOnly.Should().Be(((IList)List).IsReadOnly);
        }

        [Test]
        public void Should_provide_information_whether_list_is_of_fixed_size()
        {
            ((IList)List).IsFixedSize.Should().BeFalse();
        }

        [Test]
        public void Should_obtain_item_at_given_index()
        {
            OnlyItem.Should().Be("X");
        }

        [Test]
        public void Should_notify_about_new_item()
        {
            NewItems.Should().BeEquivalentTo("2", "W", "3");
        }

        [Test]
        public void Should_notify_about_removed_item()
        {
            OldItems.Should().BeEquivalentTo("2", "W", "3");
        }

        [Test]
        public void Should_notify_about_changed_item()
        {
            ChangedItems.Should().ContainKey("1").WhichValue.Should().Be("X");
        }

        [Test]
        public void Should_notify_about_cleared_item()
        {
            ClearedItems.Should().BeEquivalentTo("X");
        }

        [Test]
        public void Should_contain_a_given_item()
        {
            ((IList)List).Contains("0").Should().BeTrue();
        }

        [Test]
        public void Should_copy_boxed_items()
        {
            BoxedCopy.Should().HaveCount(1).And.Subject.First().Should().Be("0");
        }

        [Test]
        public void Should_copy_unboxed_items()
        {
            UnboxedCopy.Should().HaveCount(1).And.Subject.First().Should().Be("0");
        }

        [Test]
        public void Should_provide_index_of_an_item()
        {
            ((IList)List).IndexOf("0").Should().Be(0);
        }

        [Test]
        public void Should_iterate_through_all_items()
        {
            foreach (string value in (IEnumerable)List)
            {
                value.Should().Be("0");
            }
        }

        [Test]
        public void Should_not_remove_item_that_does_not_exist()
        {
            List.Remove("Z").Should().BeFalse();
        }

        [SetUp]
        public void Setup()
        {
            NewItems = new List<string>();
            OldItems = new List<string>();
            ClearedItems = new List<string>();
            List = new ObservableList<string>();
            ((IList)List).Add("1");
            ICollection<string> oldItems = null;
            List.CollectionChanged += (sender, e) =>
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
                    case NotifyCollectionChangedAction.Replace:
                        ChangedItems = e.OldItems.Cast<string>()
                            .Select((item, index) => new KeyValuePair<string, string>(item, (string)e.NewItems[index]))
                            .ToDictionary(_ => _.Key, _ => _.Value);
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
