namespace System.Text.Json.Polymorph.Attributes
{
    /// <summary>
    /// Marks base classes of inheritance hierarchies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class JsonBaseClassAttribute : Attribute
    {
        /// <summary>
        /// Defines the name of the Discriminator property that will be included in the serialized JSON for the subclasses of the marked class.
        /// If no <see cref="DiscriminatorName"/> is specified, the value of <see cref="JsonConstants.DefaultDiscriminator"/> will be used.
        /// </summary>
        public string DiscriminatorName { get; init; }
    }
}