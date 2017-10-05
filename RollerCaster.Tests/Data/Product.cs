using System;
using System.Collections.Generic;

namespace RollerCaster.Data
{
    public abstract class Product
    {
        public abstract string Name { get; set; }

        public virtual int Ordinal { get; set; }

        public virtual double Price { get; set; }

        public virtual DateTime CreatedOn { get; set; }

        public virtual ICollection<string> Categories { get; }

        public virtual IDictionary<string, string> Properties { get; }
    }
}
