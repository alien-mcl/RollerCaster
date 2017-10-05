using System.Collections.Generic;

namespace RollerCaster.Data
{
    public abstract class SpecializedProduct : Product
    {
        public virtual string Image { get; set; }

        public virtual ISpecializedProduct Related { get; set; }

        public virtual IDictionary<string, IProduct> Similar { get; }

        public virtual ICollection<IProduct> Connected { get; }
    }
}
