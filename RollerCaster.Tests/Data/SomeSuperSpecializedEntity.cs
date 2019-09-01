using System;

namespace RollerCaster.Data
{
    public class SomeSuperSpecializedEntity : SomeSpecializedEntity
    {
        private int _id;
        private string _uniqueId;

        public override int Id
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
                _uniqueId = null;
            }
        }

        public override string UniqueId
        {
            get
            {
                if (_uniqueId == null)
                {
                    _uniqueId = Id.ToString();
                }

                return _uniqueId;
            }

            set
            {
                _id = 0;
                _uniqueId = value;
                if (!String.IsNullOrEmpty(value))
                {
                    _id = Int32.Parse(value);
                }
            }
        }
    }
}
