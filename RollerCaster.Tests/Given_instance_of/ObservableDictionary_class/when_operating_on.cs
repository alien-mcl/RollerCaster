using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RollerCaster.Collections;

namespace Given_instance_of.ObservableDictionary_class
{
    [TestFixture]
    public class when_operating_on
    {
        private ObservableDictionary<string, string> Map { get; set; }

        private IDictionary<string, string> NewItems { get; set; }

        private IDictionary<string, string> OldItems { get; set; }

        private IDictionary<string, Tuple<string, string>> ChangedItems { get; set; }

        private IDictionary<string, string> ClearedItems { get; set; }

        private bool HasObservers { get; set; }

        private string OnlyValue { get; set; }

        private KeyValuePair<string, string>[] Copy { get; set; }

        public void TheTest()
        {
            HasObservers = Map.HasEventHandlerAdded;
            ((ICollection<KeyValuePair<string, string>>)Map).Add(new KeyValuePair<string, string>("B", "2"));
            ((ICollection<KeyValuePair<string, string>>)Map).Remove(new KeyValuePair<string, string>("B", "2"));
            Map["A"] = "0";
            OnlyValue = Map["A"];
            Map.Clear();
            Map.ClearCollectionChanged();
            Map["X"] = "9";
            ((ICollection<KeyValuePair<string, string>>)Map).CopyTo(Copy, 0);
        }

        [Test]
        public void Should_create_instance_with_initial_entries()
        {
            new ObservableDictionary<string, string>(new[] { new KeyValuePair<string, string>("A", "1") })
                .Should().ContainKey("A").WhichValue.Should().Be("1");
        }

        [Test]
        public void Should_provide_information_whether_map_is_read_only()
        {
            ((ICollection<KeyValuePair<string, string>>)Map).IsReadOnly.Should().BeFalse();
        }

        [Test]
        public void Should_provide_information_whether_map_has_observers()
        {
            HasObservers.Should().BeTrue();
        }

        [Test]
        public void Should_provide_correct_number_of_entries()
        {
            Map.Count.Should().Be(1);
        }

        [Test]
        public void Should_provide_keys()
        {
            Map.Keys.ShouldAllBeEquivalentTo(new[] { "X" });
        }

        [Test]
        public void Should_provide_values()
        {
            Map.Values.ShouldAllBeEquivalentTo(new[] { "9" });
        }

        [Test]
        public void Should_notify_about_added_entries()
        {
            NewItems.Should().ContainKey("B").WhichValue.Should().Be("2");
        }

        [Test]
        public void Should_notify_about_removed_entries()
        {
            OldItems.Should().ContainKey("B").WhichValue.Should().Be("2");
        }

        [Test]
        public void Should_notify_about_changed_entries()
        {
            ChangedItems.Should().ContainKey("A").WhichValue.ShouldBeEquivalentTo(new Tuple<string, string>("1", "0"));
        }

        [Test]
        public void Should_notify_about_cleared_map()
        {
            ClearedItems.Should().ContainKey("A").WhichValue.Should().Be("0");
        }

        [Test]
        public void Should_provide_value_under_a_given_key()
        {
            OnlyValue.Should().Be("0");
        }

        [Test]
        public void Should_contain_existing_key()
        {
            ((ICollection<KeyValuePair<string, string>>)Map).Contains(new KeyValuePair<string, string>("X", "9"))
                .Should().BeTrue();
        }

        [Test]
        public void Should_obtain_value()
        {
            string value;
            if (Map.TryGetValue("X", out value))
            {
                value.Should().Be("9");
            }
            else
            {
                Map.Should().ContainKey("X").WhichValue.Should().Be("9");
            }
        }

        [Test]
        public void Should_copy_entries()
        {
            Copy.ShouldAllBeEquivalentTo(new[] { new KeyValuePair<string, string>("X", "9") });
        }

        [Test]
        public void Should_iterate_through_all_entries()
        {
            foreach (KeyValuePair<string, string> entry in (IEnumerable)Map)
            {
                entry.ShouldBeEquivalentTo(new KeyValuePair<string, string>("X", "9"));
            }
        }

        [SetUp]
        public void Setup()
        {
            NewItems = new Dictionary<string, string>();
            OldItems = new Dictionary<string, string>();
            ClearedItems = new Dictionary<string, string>();
            ChangedItems = new Dictionary<string, Tuple<string, string>>();
            Copy = new KeyValuePair<string, string>[1];
            Map = new ObservableDictionary<string, string>();
            Map["A"] = "1";
            IDictionary<string, string> oldItems = null;
            Map.CollectionChanged += (sender, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (KeyValuePair<string, string> entry in e.NewItems.Cast<KeyValuePair<string, string>>())
                        {
                            NewItems[entry.Key] = entry.Value;
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (KeyValuePair<string, string> entry in e.OldItems.Cast<KeyValuePair<string, string>>())
                        {
                            OldItems[entry.Key] = entry.Value;
                        }

                        if (oldItems != null)
                        {
                            OldItems = oldItems;
                        }

                        break;
                    case NotifyCollectionChangedAction.Replace:
                        foreach (KeyValuePair<string, string> entry in e.OldItems.Cast<KeyValuePair<string, string>>())
                        {
                            ChangedItems[entry.Key] = new Tuple<string, string>(
                                entry.Value, 
                                e.NewItems.Cast<KeyValuePair<string, string>>().Where(_ => _.Key == entry.Key).Select(_ => _.Value).First());
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
