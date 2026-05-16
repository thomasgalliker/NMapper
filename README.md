# NMapper
[![Version](https://img.shields.io/nuget/v/NMapper.svg)](https://www.nuget.org/packages/NMapper)
[![Downloads](https://img.shields.io/nuget/dt/NMapper.svg)](https://www.nuget.org/packages/NMapper)
[![Buy Me a Coffee](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/thomasgalliker)

NMapper is a lightweight, explicit object mapping library for .NET.

It favors simple, testable C# mapping code over conventions, profiles, and hidden runtime behavior.

## Why NMapper?

With NMapper, mappings are just code:

- Explicit mapping classes in plain C#
- Refactor-safe and easy to debug
- Nested mappings through `IMappingContext`
- Automatic collection and array mapping
- Dependency injection support
- No convention-based property matching
- No "magic" configuration model

If you want mapping logic to stay visible, reviewable, and easy to test, NMapper is designed for that.

## Download and Install NMapper
This library is available on NuGet: https://www.nuget.org/packages/NMapper/
Use the following command to install NMapper using the NuGet Package Manager Console:

```powershell
PM> Install-Package NMapper
```

Or with the .NET CLI:

```bash
dotnet add package NMapper
```

NMapper supports .NET Standard 2.0 and higher.

## Quick Start

### 1. Define your models

```csharp
public class Person
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class PersonDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
```

### 2. Create a mapping

Implement `IMapping<TSource, TTarget>`:

```csharp
using NMapper;

public sealed class PersonMapping : IMapping<Person, PersonDto>
{
    public PersonDto Map(Person source)
    {
        return new PersonDto
        {
            Id = source.Id,
            Name = source.Name,
        };
    }
}
```

### 3. Create a mapper and map an object

```csharp
using NMapper;

var mapper = new Mapper(new PersonMapping());

var person = new Person
{
    Id = 1,
    Name = "John Doe",
};

var dto = mapper.Map<PersonDto>(person);

// dto.Id == 1
// dto.Name == "John Doe"
```

That is the core idea of NMapper:
you write the mapping once as a normal C# class, register it, and call `Map<TTarget>()`.

## Two-Way Mapping

If you want mapping in both directions, implement both interfaces on the same class:

```csharp
using NMapper;

public sealed class PersonMapping :
    IMapping<Person, PersonDto>,
    IMapping<PersonDto, Person>
{
    public PersonDto Map(Person source)
    {
        return new PersonDto
        {
            Id = source.Id,
            Name = source.Name,
        };
    }

    public Person Map(PersonDto source)
    {
        return new Person
        {
            Id = source.Id,
            Name = source.Name,
        };
    }
}
```

Usage:

```csharp
var mapper = new Mapper(new PersonMapping());

var dto = mapper.Map<PersonDto>(person);
var person2 = mapper.Map<Person>(dto);
```

## Nested Mappings

If a mapping needs to call other mappings, implement `IMappingWithContext<TSource, TTarget>`.

```csharp
public class Country
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class CountryDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class Person
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public Country? Country { get; set; }
}

public class PersonDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public CountryDto? Country { get; set; }
}

public sealed class CountryMapping : IMapping<Country, CountryDto>
{
    public CountryDto Map(Country source)
    {
        return new CountryDto
        {
            Id = source.Id,
            Name = source.Name,
        };
    }
}

public sealed class PersonMapping : IMappingWithContext<Person, PersonDto>
{
    public PersonDto Map(Person source, IMappingContext context)
    {
        return new PersonDto
        {
            Id = source.Id,
            Name = source.Name,
            Country = context.Map<CountryDto?>(source.Country),
        };
    }
}
```

Register both mappings:

```csharp
var mapper = new Mapper(
    new CountryMapping(),
    new PersonMapping());
```

Now `Person -> PersonDto` can delegate `Country -> CountryDto` to the mapper.

## Collections and Arrays

Collections and arrays are mapped automatically as long as an element mapping exists.

```csharp
var persons = new[]
{
    new Person { Id = 1, Name = "John Doe" },
    new Person { Id = 2, Name = "Jane Doe" },
};

var mapper = new Mapper(new PersonMapping());

PersonDto[] personDtos = mapper.Map<PersonDto[]>(persons);
IEnumerable<PersonDto>? personDtoEnumerable = mapper.Map<IEnumerable<PersonDto>>(persons);
HashSet<PersonDto>? personDtoSet = mapper.Map<HashSet<PersonDto>>(persons);
```

You only define the item mapping once. NMapper handles the collection conversion.
Common targets such as arrays, `List<T>`, `HashSet<T>`, `Collection<T>`, `IEnumerable<T>`, `ICollection<T>`, `IList<T>`, `IReadOnlyCollection<T>`, `IReadOnlyList<T>`, and `ISet<T>` are supported.
That includes cross-collection mapping such as array `T[]` to `IEnumerable<T>`, `List<T>` to `Collection<T>`, or `List<T>` to `HashSet<T>`.

## Polymorphic Sources

The generic overload `Map<TSource, TTarget>()` respects the runtime type of reference-type inputs.

```csharp
Person person = new Employee { Name = "Jane Doe" };

PersonDto dto = mapper.Map<Person, PersonDto>(person);
```

If an `Employee -> PersonDto` mapping is registered, NMapper will use it for the example above.

## Registration Options

### Register mappings directly

```csharp
var mapper = new Mapper(
    new PersonMapping(),
    new CountryMapping());
```

You can also register mappings after construction:

```csharp
IMapper mapper = new Mapper();

mapper.RegisterMapping(new PersonMapping());
mapper.RegisterMapping(new CountryMapping());
```

Or register a mapping delegate:

```csharp
IMapper mapper = new Mapper();

mapper.RegisterMapping<Person, PersonDto>(source => new PersonDto
{
    Id = source.Id,
    Name = source.Name,
});
```

### Register with dependency injection

NMapper integrates with `Microsoft.Extensions.DependencyInjection`.

```csharp
using Microsoft.Extensions.DependencyInjection;
using NMapper;

var services = new ServiceCollection();

services.AddMapping(options =>
{
    options.Mappings.ScanAssembly(typeof(PersonMapping).Assembly);
});

var serviceProvider = services.BuildServiceProvider();
var mapper = serviceProvider.GetRequiredService<IMapper>();
```

You can also add mappings manually:

```csharp
services.AddMapping(options =>
{
    options.Mappings.Add(new PersonMapping(), new CountryMapping());
});
```

## Per-Call Options

You can override mapping behavior per call:

```csharp
var dto = mapper.Map<PersonDto>(person, options =>
{
    options.EnableRecursionHandling = true;
});
```

This is useful when only specific mapping operations need additional safeguards.

## Recursion Handling

By default, NMapper does not track references while mapping.
That keeps mapping fast and allocation-light for simple object graphs.

If you map circular object graphs, enable recursion handling:

```csharp
var mapper = new Mapper(new MapperOptions
{
    EnableRecursionHandling = true,
    Mappings = new IMapping[]
    {
        new PersonMapping(),
        new CountryMapping(),
    }
});
```

You can also configure a maximum depth:

```csharp
var mapper = new Mapper(new MapperOptions
{
    EnableRecursionHandling = true,
    MaxDepth = 10,
    ThrowIfMaxDepthExceeded = true,
    Mappings = new IMapping[]
    {
        new PersonMapping(),
        new CountryMapping(),
    }
});
```

> [!WARNING]
> Recursion handling has a runtime cost and should only be enabled when needed.

## Exceptions

NMapper throws explicit exceptions when something is missing or invalid:

| Exception | Meaning |
|---|---|
| `DuplicateMappingException` | More than one mapping was registered for the same source and target type. |
| `MissingMappingException` | No mapping exists for the requested source and target type. |
| `MappingException` | A mapping failed during execution. |
| `AggregateException` | Multiple nested mappings failed during one operation. |

Example:

```csharp
try
{
    var dto = mapper.Map<PersonDto>(person);
}
catch (MissingMappingException ex)
{
    Console.WriteLine(ex.Message);
}
```

## Design Philosophy

NMapper treats mapping as application code, not configuration.

That means:

- Mapping behavior is explicit
- The implementation is visible in your codebase
- Debugging happens in normal C# code
- Refactoring works naturally
- Complex mappings stay maintainable because composition is explicit

If a mapping is important enough to exist, it is important enough to be code you can read.

## Thank You

Thanks to everyone who has contributed to this project.

If you find a bug or want to propose a feature, feel free to open an issue on GitHub.

We'd also like to thank [nabinked](https://www.nuget.org/profiles/nabinked) for leaving us the project name and working title NMapper.
