using System.Collections;
using System.Collections.Generic;

namespace RollerCaster.Collections
{
    public class SpecializedCollection : ICollection<string>
    {
        private readonly ICollection<string> _values = new List<string>();
        
        public int Count
        {
            get { return _values.Count; }
        }
        
        public bool IsReadOnly
        {
            get { return _values.IsReadOnly; }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(string item)
        {
            _values.Add(item);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public bool Contains(string item)
        {
            return _values.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _values.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            return _values.Remove(item);
        }
    }
}