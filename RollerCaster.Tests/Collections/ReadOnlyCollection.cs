using System;
using System.Collections;
using System.Collections.Generic;

namespace RollerCaster.Collections
{
    public class ReadOnlyCollection : IEnumerable<string>
    {
        private readonly IEnumerable<string> _values;

        public ReadOnlyCollection(IEnumerable<string> values)
        {
            _values = values ?? Array.Empty<string>();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}