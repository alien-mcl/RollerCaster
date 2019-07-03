using System;

namespace RollerCaster.Data
{
    public class SpecializedService : Service
    {
        private int _someValue;
        private int _anotherValue;
        private string _anotherValueSerialized;
        private bool _shouldSerialize;
        private bool _shouldDeserialize;

        public virtual bool IsOrdered { get; set; }

        public virtual int SomeValue
        {
            get { return _someValue; }

            set { _someValue = Math.Max(value, _someValue); }
        }

        public virtual int AnotherValue
        {
            get
            {
                if (_shouldDeserialize)
                {
                    _shouldDeserialize = false;
                    _anotherValue = Int32.Parse(_anotherValueSerialized);
                }

                return _anotherValue;
            }

            set
            {
                _anotherValue = value;
                _shouldSerialize = true;
            }
        }

        public virtual string AnotherValueSerialized
        {
            get
            {
                if (_shouldSerialize)
                {
                    _shouldSerialize = false;
                    _anotherValueSerialized = _anotherValue.ToString();
                }

                return _anotherValueSerialized;
            }

            set
            {
                _anotherValueSerialized = value;
                _shouldDeserialize = true;
            }
        }
    }
}
