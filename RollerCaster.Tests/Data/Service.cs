namespace RollerCaster.Data
{
    public class Service : Product
    {
        public override string Name
        {
            get { return ServiceName; }

            set { ServiceName = value; }
        }

        public virtual string ServiceName { get; set; }
    }
}
