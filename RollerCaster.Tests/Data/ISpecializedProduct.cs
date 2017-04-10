using System.Collections.Generic;

namespace RollerCaster.Data
{
    public interface ISpecializedProduct : IProduct
    {
        string Image { get; set; }

        ISpecializedProduct Related { get; set; }

        IDictionary<string, IProduct> Similar { get; }

        ICollection<IProduct> Connected { get; }
    }
}
