using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Polymorph.Attributes;
using System.Text.Json.Serialization;

namespace System.Text.Json.Polymorph.Converters
{
    /// <summary>
    /// Appends a discriminator property to the serialized json based on the <see cref="JsonSubClassAttribute"/> on the class,
    /// and deserializes polymorphic types with the help of it.
    /// </summary>
    /// <typeparam name="TBaseOrSubClass">The type of the class that the converter can convert</typeparam>
    public class DiscriminatorConverter<TBaseOrSubClass> : JsonConverter<TBaseOrSubClass>
    {
        private readonly string discriminatorPropertyName;
        private readonly Dictionary<string, Type> subTypes;

        public DiscriminatorConverter(string discriminatorPropertyName, params Assembly[] assembliesToSearch)
        {
            this.discriminatorPropertyName = discriminatorPropertyName;

            subTypes = assembliesToSearch.Append(typeof(TBaseOrSubClass).Assembly)
                .Distinct()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsAssignableTo(typeof(TBaseOrSubClass)) &&
                            x.GetCustomAttribute<JsonSubClassAttribute>() != null)
                .ToDictionary(x => x.GetCustomAttribute<JsonSubClassAttribute>()!.Discriminator ?? x.Name);
        }

        public override TBaseOrSubClass Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);

            var discriminatorProperty = document.RootElement.EnumerateObject().Single(x => x.NameEquals(discriminatorPropertyName));
            var discriminatorValue = discriminatorProperty.Value.GetString();
            var objectType = subTypes.Single(x => x.Key == discriminatorValue).Value;

            using var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream);
            document.WriteTo(writer);
            writer.Flush();
            var jsonString = Encoding.UTF8.GetString(stream.ToArray());

            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Remove(this);

            var childConverterType = typeof(DiscriminatorConverter<>).MakeGenericType(objectType);
            var childConverter = options.Converters.SingleOrDefault(x => x.GetType() == childConverterType);
            newOptions.Converters.Remove(childConverter);

            return (TBaseOrSubClass)JsonSerializer.Deserialize(jsonString, objectType, newOptions);
        }

        public override void Write(Utf8JsonWriter writer, TBaseOrSubClass value, JsonSerializerOptions options)
        {
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Remove(this);

            var jsonString = JsonSerializer.Serialize(value, value.GetType(), newOptions);
            using var document = JsonDocument.Parse(jsonString);

            writer.WriteStartObject();

            var valueType = value.GetType();
            var discriminatorValue = valueType.GetCustomAttribute<JsonSubClassAttribute>()?.Discriminator ?? valueType.Name;
            writer.WriteString(discriminatorPropertyName, discriminatorValue);

            foreach (var property in document.RootElement.EnumerateObject())
            {
                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }
    }
}