using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace RollerCaster.Collections
{
    /// <summary>Wraps a <see cref="List{T}" /> with an implementation of the <see cref="INotifyCollectionChanged" />.</summary>
    /// <typeparam name="T">Type of items stored.</typeparam>
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
        bool ICollection<T>.IsReadOnly { get { return _list.IsReadOnly; } }

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
                    bool isChange = false;
                    T oldValue = default(T);
                    if (CollectionChanged != null && !Equals(_list[index], value))
                    {
                        isChange = true;
                        oldValue = _list[index];
                    }

                    _list[index] = value;
                    if (isChange)
                    {
                        CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue));
                    }
                }
            }
        }

        /// <inheritdoc />
        object IList.this[int index]
        {
            get
            {
                return this[index];
            }

            set
            {
                this[index] = (T)value;
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
            AddInternal(item);
        }

        /// <inheritdoc />
        int IList.Add(object value)
        {
            return AddInternal((T)value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            NotifyCollectionChangedEventArgs e = null;
            lock (_list)
            {
                if (CollectionChanged != null)
                {
                    var items = new T[Count];
                    _list.CopyTo(items, 0);
                    e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items);
                }

                _list.Clear();
            }

            if (e != null)
            {
                CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                CollectionChanged.Invoke(this, e);
            }
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
            return Contains((T)value);
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
            lock (_list)
            {
                ((IList)_list).CopyTo(array, index);
            }
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
            return IndexOf((T)value);
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            lock (_list)
            {
                _list.Insert(index, item);
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <inheritdoc />
        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            bool result = false;
            lock (_list)
            {
                var indexOf = _list.IndexOf(item);
                if (indexOf != -1)
                {
                    _list.RemoveAt(indexOf);
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, indexOf));
                    result = true;
                }
            }

            return result;
        }

        /// <inheritdoc />
        void IList.Remove(object value)
        {
            Remove((T)value);
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

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            lock (_list)
            {
                return _list.GetEnumerator();
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int AddInternal(T item)
        {
            int result;
            lock (_list)
            {
                _list.Add(item);
                result = _list.Count;
            }

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            return result;
        }
    }
}
