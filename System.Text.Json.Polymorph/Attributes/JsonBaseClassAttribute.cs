namespace System.Text.Json.Polymorph.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class JsonBaseClassAttribute : Attribute
    {
        public string DiscriminatorPropertyName { get; init; }
    }
}