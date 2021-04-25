namespace System.Text.Json.Polymorph.Attributes
{
    /// <summary>
    /// Marks subclasses in inheritance hierarchies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class JsonSubClassAttribute : Attribute
    {
        /// <summary>
        /// Defines the value of the Discriminator property that will be included in the serialized JSON representation of the class marked by this Attribute.
        /// If no <see cref="DiscriminatorValue"/> is specified the name of the target class will be used.
        /// </summary>
        public string DiscriminatorValue { get; init; }

    }
}