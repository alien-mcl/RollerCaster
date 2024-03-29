﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using RollerCaster.Collections;
using RollerCaster.Reflection;

namespace RollerCaster
{
    /// <summary>Iterates through a collections of properties set.</summary>
    internal sealed class MulticastPropertyValueEnumerator : IEnumerator<MulticastPropertyValue>
    {
        private const int ObjectLevel = 0;
        private const int EntityTypeLevel = 2;
        private const int IsOnPropertyLevel = 3;
        private const int IsOnTypeLevel = 2;

        private readonly Stack<Tuple<IDictionary, IEnumerator>> _stack;
        private readonly ICollection<PropertyInfo> _visitedProperties;
        private readonly OrderedDictionary _properties;

        internal MulticastPropertyValueEnumerator(MulticastObject multicastObject)
        {
            _stack = new Stack<Tuple<IDictionary, IEnumerator>>(3);
            _visitedProperties = new HashSet<PropertyInfo>(PropertyInfoEqualityComparer.Default);
            var types = new List<Type>();
            foreach (var type in multicastObject.Types)
            {
                types.Add(type);
                type.EnsureDetailsOf();
                foreach (var property in DynamicExtensions.TypeProperties[type])
                {
                    multicastObject.GetProperty(property);
                }
            }

            _properties = new OrderedDictionary();
            foreach (var type in types.TopologicSort())
            {
                Dictionary<Type, Dictionary<PropertyInfo, object>> typeProperties;
                if (multicastObject.Properties.TryGetValue(type, out typeProperties))
                {
                    _properties.Add(type, multicastObject.Properties[type]);
                }
            }

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
                        case ObjectLevel:
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

                _visitedProperties.Add(property);
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
            for (int index = 0; index < 3; index++)
            {
                var current = (index == ObjectLevel
                    ? _properties
                    : (IDictionary)_stack.Peek().Item2.Current.GetType().GetProperty("Value").GetValue(_stack.Peek().Item2.Current));
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

        private static bool Equals(PropertyInfo left, PropertyInfo right)
        {
            if (!left.GetType().Name.StartsWith("Runtime", StringComparison.Ordinal)
                || !right.GetType().Name.StartsWith("Runtime", StringComparison.Ordinal))
            {
                return false;
            }

            return left.PropertyType == right.PropertyType
                && left.Name == right.Name
                && right.DeclaringType.IsAssignableFrom(left.DeclaringType);
        }

        private bool MoveNextInternal()
        {
            while (_stack.Peek().Item2.MoveNext())
            {
                if (_stack.Count == IsOnPropertyLevel)
                {
                    PropertyInfo property;
                    if ((property = ((DictionaryEntry)_stack.Peek().Item2.Current).Key as PropertyInfo) != null
                        && !_visitedProperties.Any(_ => Equals(_, property))
                        && !property.Name.StartsWith(HiddenPropertyInfo.HiddenPropertyNameIndicator, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
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
                _stack.Push(new Tuple<IDictionary, IEnumerator>(current, current.GetEnumerator()));
                return MoveNextInternal();
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
    }
}
