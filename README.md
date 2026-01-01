# NMapper
[![Version](https://img.shields.io/nuget/v/NMapper.svg)](https://www.nuget.org/packages/NMapper) [![Downloads](https://img.shields.io/nuget/dt/NMapper.svg)](https://www.nuget.org/packages/NMapper) [![Buy Me a Coffee](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/thomasgalliker)

NMapper is a lightweight, explicit object mapping library for .NET. It favours clear, testable mapping code over configuration and conventions.

### Key Features
- Explicit mapping classes written in plain C#
- One-way and two-way mappings.
- Supports nested mappings.
- Automatic mapping of collections and arrays.
- Clear compile-time errors instead of runtime surprises.
- No conventions, no reflection-based property mapping, no magic.
- Designed to use with dependency injection (Microsoft.Extensions.DependencyInjection).

### Philosophy
Mapping is application logic. NMapper treats mappings as first-class code rather than configuration.
Every mapping is explicit, discoverable, and refactor-safe.

NMapper intentionally avoids automatic property matching and hidden behavior in favor of clarity, control, and debuggability.

### Download and Install NMapper
This library is available on NuGet: https://www.nuget.org/packages/NMapper/
Use the following command to install NMapper using the NuGet Package Manager Console:

```powershell
PM> Install-Package NMapper
```

You can use this library in any .NET project which is compatible to .NET Standard 2.0 and higher.

### API Usage
#### Define a Mapping

Mappings are defined as simple C# classes by implementing `IMapping<TSource, TTarget>`.

```csharp
public class PersonMapping :
    IMapping<Person, PersonDto>,
    IMapping<PersonDto, Person>
{
    public PersonDto Map(Person source) => new()
    {
        Id = source.Id,
        Name = source.Name
    };

    public Person Map(PersonDto source) => new()
    {
        Id = source.Id,
        Name = source.Name
    };
}
```

#### Nested Mappings

If a mapping needs to delegate to other mappings, implement
`IMappingWithContext<TSource, TTarget>`.

```csharp
public class OrderMapping : IMappingWithContext<Order, OrderDto>
{
    public OrderDto Map(Order source, IMappingContext context) => new()
    {
        Id = source.Id,
        Customer = context.Map<CustomerDto>(source.Customer)
    };
}
```

#### Register Mappings

Mappings are typically registered via dependency injection.

```csharp
services.AddMapping(options =>
{
    options.MapperAssemblies = new[]
    {
        typeof(PersonMapping).Assembly
    };
});
```

#### Perform Mappings
Create a new instance of `Mapper` or inject `IMapper` via dependency injection and use it to perform mappings.

```csharp
var personDto = mapper.Map<PersonDto>(person);
```

#### Mapping Collections and Arrays
Collections and arrays are mapped automatically as long as an element mapping exists.
```csharp
var personDtos = mapper.Map<IEnumerable<PersonDto>>(persons);
```

### Thank You
A big thank you to all the people who have contributed to this project!
If you find a bug or want to propose a new feature, feel free to open an issue on GitHub.

We'd also like to thank [nabinked](https://www.nuget.org/profiles/nabinked) for leaving us the project name and working title NMapper.