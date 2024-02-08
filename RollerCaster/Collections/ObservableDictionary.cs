using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace RollerCaster.Collections
{
    /// <summary>Provides an observable thread-safe implementation of the <see cref="IDictionary{TKey, TValue}" /> interface.</summary>
    /// <typeparam name="TKey">Type of key.</typeparam>
    /// <typeparam name="TValue">Type of value.</typeparam>
    public sealed class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IObservableCollection, IDictionary
    {
        private readonly ConcurrentDictionary<TKey, TValue> _map;

        /// <summary>Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.</summary>
        public ObservableDictionary() : this(EqualityComparer<TKey>.Default)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.</summary>
        /// <param name="comparer">Key equality comparer.</param>
        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            _map = new ConcurrentDictionary<TKey, TValue>(comparer);
        }

        /// <summary>Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.</summary>
        /// <param name="entries">Initial entries.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required to maintain compatibility with common implementation.")]
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> entries) : this(entries, EqualityComparer<TKey>.Default)
        {
        }
        
        /// <summary>Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}" /> class.</summary>
        /// <param name="entries">Initial entries.</param>
        /// <param name="comparer">Key equality comparer.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required to maintain compatibility with common implementation.")]
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> entries, IEqualityComparer<TKey> comparer)
        {
            _map = new ConcurrentDictionary<TKey, TValue>(entries, comparer);
        }

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <inheritdoc />
        public int Count
        {
            get { return _map.Count; }
        }

        /// <inheritdoc />
        object ICollection.SyncRoot
        {
            get { return ((ICollection)_map).SyncRoot; }
        }

        /// <inheritdoc />
        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)_map).IsSynchronized; }
        }

        /// <inheritdoc />
        bool IDictionary.IsReadOnly
        {
            get { return ((IDictionary)_map).IsReadOnly; }
        }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return ((ICollection<KeyValuePair<TKey, TValue>>)_map).IsReadOnly; }
        }

        /// <inheritdoc />
        bool IDictionary.IsFixedSize
        {
            get { return ((IDictionary)_map).IsFixedSize; }
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys
        {
            get { return _map.Keys; }
        }

        /// <inheritdoc />
        ICollection IDictionary.Keys
        {
            get { return ((IDictionary)_map).Keys; }
        }

        /// <inheritdoc />
        public ICollection<TValue> Values
        {
            get { return _map.Values; }
        }
        
        /// <inheritdoc />
        ICollection IDictionary.Values
        {
            get { return ((IDictionary)_map).Values; }
        }

        /// <summary>Gets a value indicating whether there is an event handler added to the <see cref="CollectionChanged" />.</summary>
        public bool HasEventHandlerAdded { get { return CollectionChanged != null; } }

        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get
            {
                return _map[key];
            }

            set
            {
                TValue currentValue = default(TValue);
                NotifyCollectionChangedEventArgs e = null;
                if (CollectionChanged != null)
                {
                    if (TryGetValue(key, out currentValue))
                    {
                        if (!Equals(value, currentValue))
                        {
                            e = new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Replace,
                                new KeyValuePair<TKey, TValue>(key, value),
                                new KeyValuePair<TKey, TValue>(key, currentValue));
                        }
                    }
                    else
                    {
                        if (!Equals(value, currentValue))
                        {
                            e = new NotifyCollectionChangedEventArgs(
                                NotifyCollectionChangedAction.Add,
                                new KeyValuePair<TKey, TValue>(key, value));
                        }
                    }
                }

                _map[key] = value;
                if (e != null)
                {
                    CollectionChanged.Invoke(this, e);
                }
            }
        }
        
        /// <inheritdoc />
        object IDictionary.this[object key]
        {
            get { return this[(TKey)key]; }
            set { this[(TKey)key] = (TValue)value; }
        }

        /// <inheritdoc />
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            if (_map.TryAdd(item.Key, item.Value))
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
        }
        
        /// <inheritdoc />
        void IDictionary.Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this).Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }
        
        /// <inheritdoc />
        void IDictionary.Remove(object key)
        {
            Remove((TKey)key);
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            TValue value;
            bool result = _map.TryRemove(key, out value);
            if (result)
            {
                CollectionChanged?.Invoke(
                    this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value)));
            }

            return result;
        }

        /// <inheritdoc />
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }
        
        /// <inheritdoc />
        bool IDictionary.Contains(object key)
        {
            var result = false;
            if (key is TKey)
            {
                result = ContainsKey((TKey)key);
            }

            return result;
        }

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
            return _map.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _map.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_map).CopyTo(array, arrayIndex);
        }
        
        /// <inheritdoc />
        void ICollection.CopyTo(Array array, int index)
        {
            ((IDictionary)_map).CopyTo(array, index);
        }

        /// <inheritdoc />
        public void Clear()
        {
            NotifyCollectionChangedEventArgs e = null;
            if (CollectionChanged != null)
            {
                var items = new KeyValuePair<TKey, TValue>[Count];
                ((ICollection<KeyValuePair<TKey, TValue>>)_map).CopyTo(items, 0);
                e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items);
            }

            _map.Clear();
            if (e != null)
            {
                CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                CollectionChanged.Invoke(this, e);
            }
        }

        /// <inheritdoc />
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_map).GetEnumerator();
        }

        /// <inheritdoc />
        public void ClearCollectionChanged()
        {
            foreach (var @delegate in CollectionChanged.GetInvocationList())
            {
                CollectionChanged -= (NotifyCollectionChangedEventHandler)@delegate;
            }
        }
        
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _map.GetEnumerator();
        }
    }
}
