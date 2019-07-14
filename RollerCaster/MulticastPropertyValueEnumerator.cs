using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using RollerCaster.Reflection;

namespace RollerCaster
{
    /// <summary>Iterates through a collections of properties set.</summary>
    internal sealed class MulticastPropertyValueEnumerator : IEnumerator<MulticastPropertyValue>
    {
        private const int PropertyLevel = 0;
        private const int EntityTypeLevel = 2;
        private const int IsOnPropertyLevel = 3;
        private const int IsOnTypeLevel = 2;

        private readonly MulticastObject _multicastObject;
        private readonly Stack<Tuple<IDictionary, IEnumerator>> _stack;
        private readonly HashSet<PropertyInfo> _visitedEntityProperties;

        internal MulticastPropertyValueEnumerator(MulticastObject multicastObject)
        {
            _multicastObject = multicastObject;
            _stack = new Stack<Tuple<IDictionary, IEnumerator>>(3);
            _visitedEntityProperties = new HashSet<PropertyInfo>(PropertyInfoEqualityComparer.Default);
            Reset();
        }

        /// <inheritdoc />
        public MulticastPropertyValue Current
        {
            get
            {
                if (_stack.Count == 0)
                {
                    return null;
                }

                Type type = null;
                PropertyInfo property = null;
                object value = null;
                int index = 0;
                foreach (var item in _stack)
                {
                    switch (index)
                    {
                        case PropertyLevel:
                        {
                            var pair = (DictionaryEntry)item.Item2.Current;
                            property = (PropertyInfo)pair.Key;
                            value = pair.Value;
                            break;
                        }

                        case EntityTypeLevel:
                        {
                            var pair = (DictionaryEntry)item.Item2.Current;
                            type = (Type)pair.Key;
                            break;
                        }
                    }

                    index++;
                }

                MarkAsVisited(property);
                return new MulticastPropertyValue(type, property, value);
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("UnitTests", "TS0000:NoUnitTests", Justification = "Implemented only to meet the requirements.")]
        object IEnumerator.Current { get { return Current; } }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            return (_stack.Count >= 3 && MoveNextInternal());
        }

        /// <inheritdoc />
        public void Reset()
        {
            _stack.Clear();
            _visitedEntityProperties.Clear();
            for (int index = 0; index < 3; index++)
            {
                var current = (index == PropertyLevel
                    ? _multicastObject.Properties
                    : (IDictionary)_stack.Peek().Item2.Current.GetType().GetTypeInfo().GetDeclaredProperty("Value").GetValue(_stack.Peek().Item2.Current));
                if (!ResetInternal(current))
                {
                    return;
                }

                if (index >= EntityTypeLevel)
                {
                    continue;
                }

                if (!_stack.Peek().Item2.MoveNext())
                {
                    return;
                }
            }
        }

        private bool MoveNextInternal()
        {
            while (_stack.Peek().Item2.MoveNext()
                && (_stack.Count < IsOnPropertyLevel
                    || (_stack.Count == IsOnPropertyLevel
                        && !((PropertyInfo)((DictionaryEntry)_stack.Peek().Item2.Current).Key)
                            .Name.StartsWith(HiddenPropertyInfo.HiddenPropertyNameIndicator, StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }

            IEnumerator currentEnumerator;
            if (_stack.Count == IsOnTypeLevel)
            {
                var currentType = (Type)((DictionaryEntry)_stack.Last().Item2.Current).Key;
                var unvisitedProperties = DynamicExtensions.TypeProperties[currentType]
                    .Except(_visitedEntityProperties, PropertyInfoEqualityComparer.Default);
                if (unvisitedProperties.Any())
                {
                    var unvisitedPropertiesMap = (IDictionary)unvisitedProperties.ToDictionary(
                        _ => _,
                        _ => _multicastObject.GetProperty(_));
                    currentEnumerator = unvisitedPropertiesMap.GetEnumerator();
                    _stack.Push(new Tuple<IDictionary, IEnumerator>(unvisitedPropertiesMap, currentEnumerator));
                    return currentEnumerator.MoveNext();
                }
            }

            _stack.Pop();
            if (_stack.Count == 0)
            {
                return false;
            }

            if (!MoveNextInternal())
            {
                return false;
            }

            if (_stack.Count <= IsOnTypeLevel)
            {
                var current = (IDictionary)((DictionaryEntry)_stack.Peek().Item2.Current).Value;
                _stack.Push(new Tuple<IDictionary, IEnumerator>(current, currentEnumerator = current.GetEnumerator()));
                return currentEnumerator.MoveNext();
            }

            return true;
        }

        private bool ResetInternal(IDictionary dictionary)
        {
            var result = false;
            if (dictionary.Count != 0)
            {
                result = true;
                _stack.Push(new Tuple<IDictionary, IEnumerator>(dictionary, dictionary.GetEnumerator()));
            }

            return result;
        }

        private void MarkAsVisited(PropertyInfo propertyInfo)
        {
            _visitedEntityProperties.Add(propertyInfo);
        }
    }
}
