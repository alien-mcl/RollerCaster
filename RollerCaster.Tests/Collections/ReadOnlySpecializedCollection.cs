using System.Collections;
using System.Collections.Generic;

namespace RollerCaster.Collections
{
    public class ReadOnlySpecializedCollection : IEnumerable<string>
    {
        private readonly IEnumerable<string> _values;

        public ReadOnlySpecializedCollection(IEnumerable<string> values)
        {
            _values = values;
        }

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
