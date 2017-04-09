namespace RollerCaster.Data
{
    public interface ISpecializedProduct : IProduct
    {
        string Image { get; set; }

        IProduct Related { get; set; }
    }
}
