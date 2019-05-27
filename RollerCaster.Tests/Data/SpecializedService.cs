using System;

namespace RollerCaster.Data
{
    public class SpecializedService : Service
    {
        private int _someValue;

        public virtual bool IsOrdered { get; set; }

        public virtual int SomeValue
        {
            get { return _someValue; }

            set { _someValue = Math.Max(value, _someValue); }
        }
    }
}
