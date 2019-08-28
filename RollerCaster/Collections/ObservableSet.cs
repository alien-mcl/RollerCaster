using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RollerCaster.Collections
{
    /// <summary>Provides an observable implementation of the <see cref="ISet{T}" />.</summary>
    /// <typeparam name="T">Type of items stored within the set.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This is a set and the name corresponds to it's function.")]
    public class ObservableSet<T> : ISet<T>, IObservableCollection
    {
        private readonly ISet<T> _set;

        /// <summary>Initializes a new instance of the <see cref="ObservableSet{T}" /> class.</summary>
        public ObservableSet() : this(EqualityComparer<T>.Default)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ObservableSet{T}" /> class.</summary>
        /// <param name="comparer">Key equality comparer.</param>
        public ObservableSet(IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
        }

        /// <summary>Initializes a new instance of the <see cref="ObservableSet{T}" /> class.</summary>
        /// <param name="entries">Initial entries.</param>
        public ObservableSet(IEnumerable<T> entries) : this(entries, EqualityComparer<T>.Default)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ObservableSet{T}" /> class.</summary>
        /// <param name="entries">Initial entries.</param>
        /// <param name="comparer">Key equality comparer.</param>
        public ObservableSet(IEnumerable<T> entries, IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(entries, comparer);
        }

        /// <inheritdoc />
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <inheritdoc />
        public int Count
        {
            get { return _set.Count; }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return _set.IsReadOnly; }
        }

        /// <inheritdoc />
        public bool HasEventHandlerAdded
        {
            get { return CollectionChanged != null; }
        }

        /// <inheritdoc />
        public bool Add(T item)
        {
            bool result;
            lock (_set)
            {
                result = _set.Add(item);
            }

            if (result)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }

            return result;
        }
        
        /// <inheritdoc />
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }
        
        /// <inheritdoc />
        public bool Remove(T item)
        {
            bool result;
            lock (_set)
            {
                result = _set.Remove(item);
            }

            if (result)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }

            return result;
        }

        /// <inheritdoc />
        public void Clear()
        {
            NotifyCollectionChangedEventArgs e = null;
            lock (_set)
            {
                if (CollectionChanged != null)
                {
                    var items = new T[Count];
                    _set.CopyTo(items, 0);
                    e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items);
                }

                _set.Clear();
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
            lock (_set)
            {
                return _set.Contains(item);
            }
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_set)
            {
                _set.CopyTo(array, arrayIndex);
            }
        }
        
        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            lock (_set)
            {
                return _set.IsProperSubsetOf(other);
            }
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            lock (_set)
            {
                return _set.IsProperSupersetOf(other);
            }
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            lock (_set)
            {
                return _set.IsSubsetOf(other);
            }
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            lock (_set)
            {
                return _set.IsSupersetOf(other);
            }
        }
        
        /// <inheritdoc />
        public bool SetEquals(IEnumerable<T> other)
        {
            lock (_set)
            {
                return _set.SetEquals(other);
            }
        }
        
        /// <inheritdoc />
        public bool Overlaps(IEnumerable<T> other)
        {
            lock (_set)
            {
                return _set.Overlaps(other);
            }
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<T> other)
        {
            if (!HasEventHandlerAdded)
            {
                lock (_set)
                {
                    _set.ExceptWith(other);
                }

                return;
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            lock (_set)
            {
                if (other == this)
                {
                    Clear();
                }
                else
                {
                    var removed = new List<T>();
                    foreach (T item in other)
                    {
                        if (_set.Remove(item))
                        {
                            removed.Add(item);
                        }
                    }

                    if (removed.Count > 0)
                    {
                        CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed));
                    }
                }
            }
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (!HasEventHandlerAdded)
            {
                lock (_set)
                {
                    _set.SymmetricExceptWith(other);
                }

                return;
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            lock (_set)
            {
                var toBeRemoved = new List<T>();
                foreach (var item in _set)
                {
                    if (other.Contains(item))
                    {
                        toBeRemoved.Add(item);
                    }
                }

                var toBeAdded = new List<T>();
                foreach (var item in other)
                {
                    if (!_set.Contains(item))
                    {
                        toBeAdded.Add(item);
                    }
                }

                foreach (var removed in toBeRemoved)
                {
                    _set.Remove(removed);
                }

                foreach (var added in toBeAdded)
                {
                    _set.Add(added);
                }

                if (toBeRemoved.Count > 0)
                {
                    CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, toBeRemoved));
                }

                if (toBeAdded.Count > 0)
                {
                    CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, toBeAdded));
                }
            }
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<T> other)
        {
            if (!HasEventHandlerAdded)
            {
                lock (_set)
                {
                    _set.IntersectWith(other);
                }

                return;
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            lock (_set)
            {
                var toBeRemoved = new List<T>();
                foreach (var item in _set)
                {
                    if (!other.Contains(item))
                    {
                        toBeRemoved.Add(item);
                    }
                }

                foreach (var removed in toBeRemoved)
                {
                    _set.Remove(removed);
                }

                if (toBeRemoved.Count > 0)
                {
                    CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, toBeRemoved));
                }
            }
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<T> other)
        {
            if (!HasEventHandlerAdded)
            {
                lock (_set)
                {
                    _set.UnionWith(other);
                }

                return;
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            lock (_set)
            {
                var added = new List<T>();
                foreach (var item in other)
                {
                    if (_set.Add(item))
                    {
                        added.Add(item);
                    }
                }

                if (added.Count > 0)
                {
                    CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added));
                }
            }
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void ClearCollectionChanged()
        {
            foreach (var @delegate in CollectionChanged.GetInvocationList())
            {
                CollectionChanged -= (NotifyCollectionChangedEventHandler)@delegate;
            }
        }
    }
}
