using System.Linq;
using System.Reflection;
using System.Text.Json.Polymorph.Attributes;
using System.Text.Json.Polymorph.Converters;
using System.Text.Json.Serialization;

namespace System.Text.Json.Polymorph.Extensions
{
    public static class JsonOptionsExtensions
    {
        /// <summary>
        /// Adds the necessary converters to convert the subclasses of <typeparamref name="TBaseClass"/> with the configured discriminators.
        /// </summary>
        /// <typeparam name="TBaseClass">The root type of the class hierarchy</typeparam>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> instance to add the converters to</param>
        /// <param name="discriminatorPropertyName">The name of the discriminator property that is going to be serialized to the subclasses' JSON representation</param>
        /// <param name="assembliesToSearch">Assemblies that will be searched for subclasses of <typeparamref name="TBaseClass"/> with <see cref="JsonSubClassAttribute"/>.
        /// The Assembly of <typeparamref name="TBaseClass"/> is automatically included to the searched assemblies.</param>
        public static void AddDiscriminatorConverterForHierarchy<TBaseClass>(
            this JsonSerializerOptions options,
            string discriminatorPropertyName = JsonConstants.DefaultDiscriminator,
            params Assembly[] assembliesToSearch)
        {
            var converters = typeof(TBaseClass).Assembly.GetTypes()
                .Where(x => x.IsAssignableTo(typeof(TBaseClass)))
                .Select(x => (JsonConverter)Activator.CreateInstance(
                    typeof(DiscriminatorConverter<>).MakeGenericType(x),
                    discriminatorPropertyName,
                    assembliesToSearch));

            foreach (var converter in converters)
            {
                options.Converters.Add(converter);
            }
        }

        /// <summary>
        /// Adds the necessary converters to convert the subclasses of all types with the attribute <see cref="JsonBaseClassAttribute"/> in the given assemblies.
        /// </summary>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> instance to add the converters to</param>
        /// <param name="assemblies">Assemblies that will be searched for classes with <see cref="JsonBaseClassAttribute"/>
        /// and subclasses with <see cref="JsonSubClassAttribute"/></param>
        public static void AddDiscriminatorConverters(this JsonSerializerOptions options,
            params Assembly[] assemblies)
        {
            var baseClasses = assemblies.SelectMany(x => x.GetTypes())
                .Where(x => x.GetCustomAttribute<JsonBaseClassAttribute>() != null);

            foreach (var baseClass in baseClasses)
            {
                typeof(JsonOptionsExtensions)
                    .GetMethod(nameof(AddDiscriminatorConverterForHierarchy))!
                    .MakeGenericMethod(baseClass)
                    .Invoke(null, new object[]
                    {
                        options,
                        baseClass.GetCustomAttribute<JsonBaseClassAttribute>()!.DiscriminatorName ?? JsonConstants.DefaultDiscriminator,
                        assemblies
                    });
            }
        }
    }
}