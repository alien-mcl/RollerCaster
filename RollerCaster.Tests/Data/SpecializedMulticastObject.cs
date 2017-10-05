namespace RollerCaster.Data
{
    public class SpecializedMulticastObject : MulticastObject
    {
        /// <summary>Gets or sets the identifier of this instance..</summary>
        public string Id { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var anotherInstance = obj as SpecializedMulticastObject;
            return anotherInstance != null && Id.Equals(anotherInstance.Id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
