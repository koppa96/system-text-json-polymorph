namespace System.Text.Json.Polymorph.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class JsonSubClassAttribute : Attribute
    {
        public string Discriminator { get; init; }

    }
}