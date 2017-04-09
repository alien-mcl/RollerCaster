using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RollerCaster.Reflection;

namespace RollerCaster
{
    /// <summary>Iterates through a collections of properties set.</summary>
    internal sealed class MulticastPropertyValueEnumerator : IEnumerator<MulticastPropertyValue>
    {
        private readonly MulticastObject _multicastObject;
        private Stack<Tuple<IDictionary, IEnumerator>> _stack;

        internal MulticastPropertyValueEnumerator(MulticastObject multicastObject)
        {
            _multicastObject = multicastObject;
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
                string propertyName = null;
                object value = null;
                int index = 0;
                foreach (var item in _stack)
                {
                    switch (index)
                    {
                        case 0:
                        {
                            var pair = (DictionaryEntry)item.Item2.Current;
                            propertyName = (string)pair.Key;
                            value = pair.Value;
                            break;
                        }

                        case 2:
                        {
                            var pair = (DictionaryEntry)item.Item2.Current;
                            property = (type = (Type)pair.Key).FindProperty(propertyName);
                            break;
                        }
                    }

                    index++;
                }

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
            _stack = new Stack<Tuple<IDictionary, IEnumerator>>(3);
            for (int index = 0; index < 3; index++)
            {
                var current = (index == 0
                    ? _multicastObject.Properties
                    : (IDictionary)_stack.Peek().Item2.Current.GetType().GetTypeInfo().GetDeclaredProperty("Value").GetValue(_stack.Peek().Item2.Current));
                if (!ResetInternal(current))
                {
                    return;
                }

                if (index >= 2)
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
            if (_stack.Peek().Item2.MoveNext())
            {
                return true;
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

            var current = (IDictionary)((DictionaryEntry)_stack.Peek().Item2.Current).Value;
            IEnumerator currentEnumerator;
            _stack.Push(new Tuple<IDictionary, IEnumerator>(current, currentEnumerator = current.GetEnumerator()));
            return currentEnumerator.MoveNext();
        }

        private bool ResetInternal(IDictionary dictionary)
        {
            if (dictionary.Count == 0)
            {
                return false;
            }

            _stack.Push(new Tuple<IDictionary, IEnumerator>(dictionary, dictionary.GetEnumerator()));
            return true;
        }
    }
}
