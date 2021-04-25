# Polymorph extensions for System.Text.Json
This library contains utilities for serializing and deserializing subclasses of type hierarchies,
while keeping type information and also keeping your model free from discriminator properties.
The library uses a set of Attributes to achiveve this. You can customize the name of the discriminator property in the generated JSON string
and also the values for the discriminator properties for each subclass.

The customization of the discriminator property values lets you create meaningful discriminator values that you can easily use in your
client that was not created with .NET.

## Usage

### Configuring type hierarchy
Let's say you have a class hiearchy like this:
```cs
public class BaseDto
{
    public int BaseDtoProperty { get; set; }
}

public class SubDto1 : BaseDto
{
    public string SubDtoProperty1 { get; set; }
}

public class SubDto2 : BaseDto
{
    public long SubDtoProperty2 { get; set; }
}

public class SubDto3 : BaseDto
{
    public bool SubDtoProperty3 { get; set; }
}
```

You can add the attributes defined in the library, to set up serialization:
```cs
// DiscriminatorPropertyName is optional, it defaults to "discriminator"
[JsonBaseClass(DiscriminatorName = "myAwesomeDiscriminator")]
public class BaseDto
{
    public int BaseDtoProperty { get; set; }
}

// DiscriminatorName is optional, it defaults to the name of the subclass.
// You can omit it if you use a .NET Client and have the concrete types
// Or if you simply don't mind having class names as discriminators
[JsonSubClass(DiscriminatorValue = "Sub1")]
public class SubDto1 : BaseDto
{
    public string SubDtoProperty1 { get; set; }
}

[JsonSubClass(DiscriminatorValue = "Sub2")]
public class SubDto2 : BaseDto
{
    public long SubDtoProperty2 { get; set; }
}

[JsonSubClass]
public class SubDto3 : BaseDto
{
    public bool SubDtoProperty3 { get; set; }
}
```

### Configuring the serialization with ASP.NET Core
If you are using ASP.NET Core, you can add your type hierarchies via reflection at the `ConfigureServices` method:
```cs
services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.AddDiscriminatorConverters(
            Assembly.Load("Your.Assembly.Containing.The.Classes"),
            Assembly.Load("An.Other.Assembly.Containing.Classes")
        );
    });
```

If you don't want the library to detect your hierarchies with reflection, you can use the following extension method:
```cs
services.AddControllers()
    .AddJsonOptions(options =>
    {
        // In this case you don't need to place the JsonBaseClassAttribute on your BaseDto
        // You might need to specifiy the assemblies where your subclasses were declared
        // The libary will look for subclasses in the same Assembly as the BaseDto
        options.JsonSerializerOptions.AddDiscriminatorConverterForHierarchy<BaseDto>("myAwesomeDiscriminator");
    });
```

And you are done. The following example will show how to use this in Controllers:

```cs
[Route("discriminator-test")]
[ApiController]
public class DiscriminatorTestController : ControllerBase
{
    [HttpGet]
    public BaseDto Get()
    {
        return new SubDto2 { SubDtoProperty2 = 1 };
    }
    
    [HttpPost]
    public void Post(BaseDto dto)
    {
        if (dto is SubDto1 subDto)
        {
            Console.WriteLine(subDto.SubDtoProperty1);
        }
    }
}
```

If we send a GET request to this Controller we will get the following response:
```json
{
  "myAwesomeDiscriminator": "Sub2",
  "subDtoProperty2": 1
}
```

If we send a POST request to this controller, with the content below, it is going to log `foo` to the console:
```json
{
  "myAwesomeDiscriminator": "Sub1",
  "subDtoProperty1": "foo"
}
```

### Configuring the serialization
If you are using the `JsonSerializer` class for serializing and deserializing your classes,
you can call either the `AddDiscriminatorConverters` or the `AddDiscriminatorConverterForHierarchy<>` extension method to add your converters,
like in the example with ASP.NET Core.

```cs
var dto = ...;
var options = new JsonSerializerOptions();
options.AddDiscriminatorConverters(
    Assembly.Load("Your.Assembly.Containing.The.Classes"),
    Assembly.Load("An.Other.Assembly.Containing.Classes")
);

var json = JsonSerializer.Serialize(dto, options);
```
