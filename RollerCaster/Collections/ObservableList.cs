using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace RollerCaster.Collections
{
    /// <summary>Wraps a <see cref="List{T}" /> with an implementation of the <see cref="INotifyCollectionChanged" />.</summary>
    /// <typeparam name="T">Type of items stored.</typeparam>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("UnitTests", "TS0000:NoUnitTests", Justification = "This acts as an observable wrapper around a real list.")]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Semantically it is more than a collection.")]
    public sealed class ObservableList<T> : IList<T>, IList, IObservableCollection
    {
        private readonly IList<T> _list;

        /// <summary>Initializes a new instance of the <see cref="ObservableList{T}" /> class.</summary>
        public ObservableList()
        {
            _list = new List<T>();
        }

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Gets a value indicating whether there is an event handler added to the <see cref="CollectionChanged" />.</summary>
        public bool HasEventHandlerAdded { get { return CollectionChanged != null; } }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                lock (_list)
                {
                    return _list.Count;
                }
            }
        }

        /// <inheritdoc />
        bool ICollection<T>.IsReadOnly { get { return ((ICollection<T>)_list).IsReadOnly; } }

        /// <inheritdoc />
        object ICollection.SyncRoot { get { return ((ICollection)_list).SyncRoot; } }

        /// <inheritdoc />
        bool ICollection.IsSynchronized { get { return ((ICollection)_list).IsSynchronized; } }

        /// <inheritdoc />
        bool IList.IsReadOnly { get { return ((IList)_list).IsReadOnly; } }

        /// <inheritdoc />
        bool IList.IsFixedSize { get { return ((IList)_list).IsFixedSize; } }

        /// <inheritdoc />
        public T this[int index]
        {
            get
            {
                lock (_list)
                {
                    return _list[index];
                }
            }

            set
            {
                lock (_list)
                {
                    _list[index] = value;
                }
            }
        }

        /// <inheritdoc />
        object IList.this[int index]
        {
            get
            {
                lock (_list)
                {
                    return ((IList)_list)[index];
                }
            }

            set
            {
                lock (_list)
                {
                    ((IList)_list)[index] = value;
                }
            }
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
        public void Add(T item)
        {
            lock (_list)
            {
                _list.Add(item);
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { item }));
        }

        /// <inheritdoc />
        int IList.Add(object value)
        {
            return ((IList)_list).Add(value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            var removed = new T[Count];
            lock (_list)
            {
                _list.CopyTo(removed, 0);
                _list.Clear();
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            lock (_list)
            {
                return _list.Contains(item);
            }
        }

        /// <inheritdoc />
        bool IList.Contains(object value)
        {
            return ((IList)_list).Contains(value);
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_list)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc />
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)_list).CopyTo(array, index);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            lock (_list)
            {
                return _list.IndexOf(item);
            }
        }

        /// <inheritdoc />
        int IList.IndexOf(object value)
        {
            return ((IList)_list).IndexOf(value);
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            lock (_list)
            {
                _list.Insert(index, item);
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { item }, index));
        }

        /// <inheritdoc />
        void IList.Insert(int index, object value)
        {
            ((IList)_list).Insert(index, value);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            lock (_list)
            {
                var indexOf = _list.IndexOf(item);
                if (indexOf != -1)
                {
                    _list.RemoveAt(indexOf);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { item }, indexOf));
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        void IList.Remove(object value)
        {
            ((IList)_list).Remove(value);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            T item = default(T);
            lock (_list)
            {
                item = _list[index];
                _list.RemoveAt(index);
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { item }, index));
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }
    }
}
