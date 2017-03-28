using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RollerCaster
{
    internal sealed class MulticastPropertyValueCollection : IEnumerable<MulticastPropertyValue>
    {
        private readonly MulticastObject _multicastObject;

        internal MulticastPropertyValueCollection(MulticastObject multicastObject)
        {
            _multicastObject = multicastObject;
        }

        /// <summary>Returns an enumerator that iterates through the collection of properties set.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection of properties set.</returns>
        public IEnumerator<MulticastPropertyValue> GetEnumerator()
        {
            return new MulticastPropertyValueEnumerator(_multicastObject);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("UnitTests", "TS0000:NoUnitTests", Justification = "Implemented only to match the requirements. No special logic to be tested here.")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
