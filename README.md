# NMapper
[![Version](https://img.shields.io/nuget/v/NMapper.svg)](https://www.nuget.org/packages/NMapper) [![Downloads](https://img.shields.io/nuget/dt/NMapper.svg)](https://www.nuget.org/packages/NMapper) [![Buy Me a Coffee](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/thomasgalliker)

NMapper is a lightweight, explicit object mapping library for .NET. It favours clear, testable mapping code over configuration and conventions.

## Key Features
- Explicit mapping classes written in plain C#
- One-way and two-way mappings.
- Supports nested mappings.
- Automatic mapping of collections and arrays.
- Clear compile-time errors instead of runtime surprises.
- No conventions, no reflection-based property mapping, no magic.
- Designed to use with dependency injection (Microsoft.Extensions.DependencyInjection).

## Philosophy
Mapping is application logic. NMapper treats mappings as first-class code rather than configuration.
Every mapping is explicit, discoverable, and refactor-safe.

NMapper intentionally avoids automatic property matching and hidden behavior in favor of clarity, control, and debuggability.

## Download and Install NMapper
This library is available on NuGet: https://www.nuget.org/packages/NMapper/
Use the following command to install NMapper using the NuGet Package Manager Console:

```powershell
PM> Install-Package NMapper
```

You can use this library in any .NET project which is compatible to .NET Standard 2.0 and higher.

## API Usage
### Define a Mapping
A mapping is a concrete specificiation of a source-to-target type conversion. Mappings are defined as simple C# classes by implementing `IMapping<TSource, TTarget>`.

```csharp
 public class PersonMapping :
        IMapping<Person, PersonDto>,
        IMapping<PersonDto, Person>
{
    public PersonDto Map(Person person) => new PersonDto
    {
        Id = person.Id,
        Name = person.Name,
    };

    public Person Map(PersonDto personDto) => new Person
    {
        Id = personDto.Id,
        Name = personDto.Name,
    };
}
```

### Nested Mappings
If a mapping needs to delegate to other mappings, implement `IMappingWithContext<TSource, TTarget>`. This interface gives you the extra parameter `IMappingContext` which allows to run further mappings. 

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

### Register Mappings
Each `Mapper` instance can be configured with 1-N mappings.

#### Register Mappings manually
The constructor of `Mapper` allows to specify mappings directly.

```csharp
var mappings = new IMapping[]
{
    new PersonMapping(),
    new CountryMapping(),
};
IMapper mapper = new Mapper(mappings);
```

#### Register Mappings via Dependency Injection
Mappings can be registered via dependency injection.
Use the `AddMapping` extension methods on your DI service collection.
From there you have multiple ways to register/scan mappings:

```csharp
services.AddMapping(o =>
{
    // Register all mappings from a specific assembly:
    o.Mappings.ScanAssembly(typeof(Person).Assembly);

    // Register mappings manually
    o.Mappings.Add(new IMapping[] { new PersonMapping(), new VenueMapping() });
});
```

### Perform Mappings
Create a new instance of `Mapper` or inject `IMapper` via dependency injection and use it to perform mappings.

```csharp
var personDto = mapper.Map<PersonDto>(person);
```

### Mapping Collections and Arrays
Collections and arrays are mapped automatically as long as an element mapping exists.
```csharp
var personDtos = mapper.Map<IEnumerable<PersonDto>>(persons);
```

The concrete collection instances (arrays and lists) are created using an `ICollectionFactory`.
By default, NMapper uses an internal FastCollectionFactory, which is optimized for performance. You can override this behavior via `MapperOptions.CollectionFactory`.

### Recursion Handling (Circular Object Graphs)
By default, NMapper does not track object references during mapping. This keeps the mapper fast and allocation-free for simple object graphs.
For object graphs that contain circular references (e.g. parent-child relationships with back-references), recursion handling can be enabled.

```csharp
var mapperOptions = new MapperOptions
{
    EnableRecursionHandling = true
};
```

When enabled, NMapper tracks previously mapped source objects and reuses them internally to avoid infinite recursion and stack overflows.

> [!WARNING]
> Enabling recursion handling has a measurable runtime cost and should only be enabled when required.

#### Maximum Depth
In addition to reference tracking, a maximum traversal depth can be configured:

```csharp
var mapperOptions = new MapperOptions
{
    EnableRecursionHandling = true,
    MaxDepth = 10,
    ThrowIfMaxDepthExceeded = true
};
```

`MaxDepth = 0` disables depth checking (default).
When `ThrowIfMaxDepthExceeded` is enabled, the mapper throws a `MappingException` once the depth limit is exceeded.

### Per-Call Mapping Options
Some options are also available on a per-call basis.
This allows you to enable recursion handling for specific mapping calls only while using the performance advantage for the rest of the mappings.

```csharp
var venueDto = mapper.Map<VenueDto>(venue, o => o.EnableRecursionHandling = true);
```

### Exceptions
NMapper uses explicit, strongly typed exceptions to make mapping errors easy to diagnose. All exceptions are thrown at runtime and indicate configuration or mapping logic errors.

| Exception                     | Description |
|-------------------------------|-------------|
| **DuplicateMappingException** | Thrown when more than one mapping is registered for the same source and target type. Each source → target pair must be unique. |
| **MissingMappingException**   | Thrown when no mapping exists for the requested source and target type and no built-in primitive conversion applies. |
| **MappingException**          | Thrown when an error occurs during execution of a mapping. This exception wraps the original exception and adds source type, target type, and mapping type information. |
| **AggregateException**        | When multiple nested mappings fail during a single mapping operation, NMapper may throw an `AggregateException` containing one or more of the exceptions listed above. This behavior allows all mapping errors to be reported at once instead of failing on the first error. |
| **StackOverflowException**    | Detected by the .NET runtime. Typically happens when a parent-child object graph with back-references is used while recursion handling is disabled. Set `EnableRecursionHandling = true` and try again. |


## Thank You
A big thank you to all the people who have contributed to this project!
If you find a bug or want to propose a new feature, feel free to open an issue on GitHub.

We'd also like to thank [nabinked](https://www.nuget.org/profiles/nabinked) for leaving us the project name and working title NMapper.