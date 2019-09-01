using System;

namespace RollerCaster.Data
{
    public class SomeSuperDuperSpecializedEntity : SomeSuperSpecializedEntity
    {
        private int _relatedId;
        private string _uniqueId;

        public virtual int RelatedId
        {
            get
            {
                return _relatedId;
            }

            set
            {
                _relatedId = value;
                _uniqueId = null;
            }
        }

        /// <inheritdoc />
        public override string UniqueId
        {
            get
            {
                if (_uniqueId == null)
                {
                    _uniqueId = $"{base.UniqueId}_{_relatedId}";
                }

                return _uniqueId;
            }

            set
            {
                _relatedId = 0;
                if (!String.IsNullOrEmpty(value))
                {
                    base.UniqueId = value.Split('_')[0];
                    _relatedId = Int32.Parse(value.Split('_')[1]);
                }
            }
        }
    }
}
