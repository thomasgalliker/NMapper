using System.Collections.ObjectModel;
using FluentAssertions;
using NMapper.TestData;
using NMapper.TestData.Mappings;
using Xunit;

namespace NMapper.Tests
{
    public partial class MapperTests
    {
        [Fact]
        public void ShouldCreateMapper()
        {
            // Act
            var mapper = new Mapper();

            // Assert
            mapper.Mappings.Should().BeEmpty();
        }

        [Fact]
        public void ShouldMap_ThrowsMissingMappingException()
        {
            // Arrange
            var mappings = Array.Empty<IMapping>();
            IMapper mapper = new Mapper(mappings);

            var person = new Person
            {
                Id = 1,
                Name = "John Doe",
            };

            // Act
            Action action = () => mapper.Map<PersonDto>(person);

            // Assert
            action.Should().Throw<MissingMappingException>();
        }

        [Fact]
        public void ShouldMap_CountryToCountryDto()
        {
            // Arrange
            var mappings = new[] { new CountryMapping() };
            IMapper mapper = new Mapper(mappings);

            var country = new Country
            {
                Id = 1,
                Name = "Switzerland",
                NativeName = "Schweiz",
            };

            // Act
            var countryDto = mapper.Map<CountryDto>(country);

            // Assert
            countryDto.Should().NotBeNull();
            countryDto.Id.Should().Be(country.Id);
            countryDto.Name.Should().Be(country.Name);
        }

        [Fact]
        public void ShouldMap_PersonToPersonDto_WithNestedCountryToCountryDto()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new CountryMapping(),
                new PersonMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var person = new Person
            {
                Id = 1,
                Name = "John Doe",
                CountryId = 2,
                Country = new Country
                {
                    Id = 2,
                    Name = "USA",
                    NativeName = "United States of America",
                }
            };

            // Act
            var personDto = mapper.Map<PersonDto>(person);

            // Assert
            personDto.Should().NotBeNull();
            personDto.Id.Should().Be(person.Id);
            personDto.Name.Should().Be(person.Name);
            personDto.Country.Should().BeEquivalentTo(new CountryDto
            {
                Id = 2,
                Name = "USA"
            });
        }

        [Fact]
        public void ShouldRegisterMapping()
        {
            // Arrange
            IMapper mapper = new Mapper();

            // Act
            mapper.RegisterMapping(new PersonMapping());

            // Assert
            mapper.Mappings.Should().HaveCount(1);
        }

        [Fact]
        public void ShouldRegisterMappings()
        {
            // Arrange
            IMapper mapper = new Mapper();
            var mappings = new IMapping[]
            {
                new CountryMapping(),
                new PersonMapping(),
            };

            // Act
            mapper.RegisterMappings(mappings);

            // Assert
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Country) && x.TargetType == typeof(CountryDto));
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Person) && x.TargetType == typeof(PersonDto));
        }

        [Fact]
        public void ShouldRegisterMappingWithDelegate()
        {
            // Arrange
            IMapper mapper = new Mapper();

            // Act
            mapper.RegisterMapping<Person, PersonDto>(p => new PersonDto());

            // Assert
            mapper.Mappings.Should().HaveCount(1);
        }

        [Fact]
        public void ShouldMap_PersonToPersonDto_WithNestedCountryNull()
        {
            // Arrange
            IMapper mapper = new Mapper();
            var mappings = new IMapping[]
            {
                new CountryMapping(),
                new PersonMapping(),
            };
            mapper.RegisterMappings(mappings);

            var person = new Person
            {
                Id = 1,
                Name = "John Doe",
                CountryId = 0,
                Country = null,
            };

            // Act
            var personDto = mapper.Map<PersonDto>(person);

            // Assert
            personDto.Should().NotBeNull();
            personDto.Id.Should().Be(person.Id);
            personDto.Name.Should().Be(person.Name);
            personDto.Country.Should().BeNull();
        }

        [Fact]
        public void ShouldMapCollections_ArrayToArray()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new CountryMapping(),
                new PersonMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var country = new Country
            {
                Id = 1,
                Name = "Canada",
                NativeName = "Canada",
            };
            var personsCount = 3;
            var persons = Enumerable.Range(1, personsCount)
                .Select(i => new Person
                {
                    Id = i,
                    Name = $"Person {i}",
                    CountryId = country.Id,
                    Country = country,
                    Address = null,
                }).ToArray();

            // Act
            var personDtos = mapper.Map<PersonDto[]>(persons);

            // Assert
            personDtos.Should().NotBeNull();
            personDtos.Should().HaveCount(personsCount);
            personDtos.All(p => p.Name?.StartsWith("Person") == true).Should().BeTrue();
            personDtos.All(p => p.Id > 0).Should().BeTrue();
        }

        [Fact]
        public void ShouldMapCollections_ArrayToArray_ThrowsMissingMappingException()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new PersonMapping()
            };
            IMapper mapper = new Mapper(mappings);

            var country = new Country
            {
                Id = 1,
                Name = "Canada",
                NativeName = "Canada",
            };
            var personsCount = 3;
            var persons = Enumerable.Range(1, personsCount)
                .Select(i => new Person
                {
                    Id = i,
                    Name = $"Person {i}",
                    CountryId = country.Id,
                    Country = country,
                    Address = new Address
                    {
                        Street = "123 Main St",
                        Place = "Toronto",
                        ZipCode = 12345
                    }
                }).ToArray();

            // Act
            Action action = () => mapper.Map<PersonDto[]>(persons);

            // Assert
            var aggregateException = action.Should().Throw<AggregateException>().Which;
            aggregateException.InnerExceptions.Should().HaveCount(2);
            aggregateException.InnerExceptions.All(ex => ex is MissingMappingException).Should().BeTrue();
            aggregateException.InnerExceptions[0].Message.Should().Contain("No mapping registered for Address to String");
            aggregateException.InnerExceptions[1].Message.Should().Contain("No mapping registered for Country to CountryDto");
        }

        [Fact]
        public void ShouldMapCollections_ArrayToArray_ThrowsAggregateException()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new PersonNestedExceptionsMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var country = new Country
            {
                Id = 1,
                Name = "Canada",
                NativeName = "Canada",
            };
            var personsCount = 3;
            var persons = Enumerable.Range(1, personsCount)
                .Select(i => new Person
                {
                    Id = i,
                    Name = $"Person {i}",
                    CountryId = country.Id,
                    Country = country,
                }).ToArray();

            // Act
            Action action = () => mapper.Map<double?[]>(persons);

            // Assert
            var aggregateException = action.Should().Throw<AggregateException>().Which;
            aggregateException.InnerExceptions.Should().HaveCount(2);
            aggregateException.InnerExceptions.All(ex => ex is MissingMappingException).Should().BeTrue();
            aggregateException.InnerExceptions[0].Message.Should().Contain("No mapping registered for Person to Int32");
            aggregateException.InnerExceptions[1].Message.Should().Contain("No mapping registered for Person to Single");
        }

        [Fact]
        public void ShouldMapCollections_EnumerableToEnumerable()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new CountryMapping(),
                new PersonMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var country = new Country
            {
                Id = 1,
                Name = "Canada",
                NativeName = "Canada",
            };
            var personsCount = 10;
            var persons = Enumerable.Range(1, personsCount)
                .Select(i => new Person
                {
                    Id = i,
                    Name = $"Person {i}",
                    CountryId = country.Id,
                    Country = country,
                });

            // Act
            var personDtos = mapper.Map<IEnumerable<PersonDto>>(persons);

            // Assert
            personDtos.Should().NotBeNull();
            personDtos.Should().HaveCount(personsCount);
            personDtos.All(p => p.Name?.StartsWith("Person") == true).Should().BeTrue();
            personDtos.All(p => p.Id > 0).Should().BeTrue();
        }

        [Fact]
        public void ShouldMapCollections_ListToHashSet()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new CountryMapping(),
                new PersonMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var country = new Country
            {
                Id = 1,
                Name = "Canada",
                NativeName = "Canada",
            };
            var persons = Enumerable.Range(1, 3)
                .Select(i => new Person
                {
                    Id = i,
                    Name = $"Person {i}",
                    CountryId = country.Id,
                    Country = country,
                })
                .ToList();

            // Act
            var personDtos = mapper.Map<HashSet<PersonDto>>(persons);

            // Assert
            personDtos.Should().NotBeNull();
            personDtos.Should().HaveCount(3);
            personDtos.All(p => p.Name?.StartsWith("Person") == true).Should().BeTrue();
        }

        [Fact]
        public void ShouldMapCollections_ArrayToCollection()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new CountryMapping(),
                new PersonMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var country = new Country
            {
                Id = 1,
                Name = "Canada",
                NativeName = "Canada",
            };
            var persons = Enumerable.Range(1, 3)
                .Select(i => new Person
                {
                    Id = i,
                    Name = $"Person {i}",
                    CountryId = country.Id,
                    Country = country,
                })
                .ToArray();

            // Act
            var personDtos = mapper.Map<Collection<PersonDto>>(persons);

            // Assert
            personDtos.Should().NotBeNull();
            personDtos.Should().HaveCount(3);
            personDtos.All(p => p.Name?.StartsWith("Person") == true).Should().BeTrue();
        }

        [Fact]
        public void ShouldMapCollections_ListToISet()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new CountryMapping(),
                new PersonMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var country = new Country
            {
                Id = 1,
                Name = "Canada",
                NativeName = "Canada",
            };
            var persons = Enumerable.Range(1, 3)
                .Select(i => new Person
                {
                    Id = i,
                    Name = $"Person {i}",
                    CountryId = country.Id,
                    Country = country,
                })
                .ToList();

            // Act
            var personDtos = mapper.Map<ISet<PersonDto>>(persons);

            // Assert
            personDtos.Should().NotBeNull();
            personDtos.Should().HaveCount(3);
            personDtos.All(p => p.Name?.StartsWith("Person") == true).Should().BeTrue();
        }

        [Fact]
        public void ShouldMapEnum()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new SourceTargetEnumMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var sourceEnum = SourceEnum.Second;

            // Act
            var targetEnum = mapper.Map<TargetEnum>(sourceEnum);

            // Assert
            targetEnum.Should().Be(TargetEnum.Second);
        }

        [Fact]
        public void ShouldMapUsingDelegateMapping()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new DelegateMapping<decimal, double>(d => (double)d),
            };
            IMapper mapper = new Mapper(mappings);

            var decimalValue = 123.45m;

            // Act
            var doubleValue = mapper.Map<double>(decimalValue);

            // Assert
            doubleValue.Should().Be(123.45d);
        }

        [Fact]
        public void ShouldMapNullableDecimalToDouble_NaN()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new NullableDecimalToDoubleMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            decimal? decimalValue = null;

            // Act
            var doubleValue = mapper.Map<decimal?, double>(decimalValue);

            // Assert
            doubleValue.Should().Be(double.NaN);
        }

        [Fact]
        public void ShouldMapNullableDecimalToDouble_Value()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new NullableDecimalToDoubleMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            decimal? decimalValue = 10m;

            // Act
            var doubleValue = mapper.Map<decimal?, double>(decimalValue);

            // Assert
            doubleValue.Should().Be(10d);
        }

        [Fact]
        public void ShouldMap_ThrowsMappingException()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new PersonNotImplementedMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var person = new Person
            {
                Id = 1,
                Name = "John Doe",
            };

            // Act
            Action action = () => mapper.Map<string>(person);

            // Assert
            action.Should().Throw<MappingException>().WithInnerException<NotImplementedException>();
        }

        [Fact]
        public void ShouldMapWithContext_ThrowsMappingException()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new PersonNestedExceptionsMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var person = new Person
            {
                Id = 1,
                Name = "John Doe",
            };

            // Act
            Action action = () => mapper.Map<double?>(person);

            // Assert
            var aggregateException = action.Should().Throw<AggregateException>().Which;
            aggregateException.InnerExceptions.Should().HaveCount(2);
            aggregateException.InnerExceptions.All(ex => ex is MissingMappingException).Should().BeTrue();
            aggregateException.InnerExceptions[0].Message.Should().Contain("No mapping registered for Person to Int32");
            aggregateException.InnerExceptions[1].Message.Should().Contain("No mapping registered for Person to Single");
        }

        [Fact(Skip = "Run manually")]
        public void ShouldMap_WithRecursion_ThrowsStackoverflowException()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                EnableRecursionHandling = false,
                Mappings = new IMapping[]
                {
                    new VenueMapping(),
                    new WaterAreaMapping(),
                }
            };

            IMapper mapper = new Mapper(mapperOptions);

            var venue = Venue.GetRecursiveVenueTestData();

            // Act
            mapper.Map<VenueDto>(venue);

            // Assert
            // Debug and observe the StackOverflowException
        }

        [Fact]
        public void ShouldMap_WithRecursion()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new VenueMapping(),
                    new WaterAreaMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var venue = Venue.GetRecursiveVenueTestData();

            // Act
            var venueDto = mapper.Map<VenueDto>(venue);

            // Assert
            venueDto.Should().NotBeNull();
            venueDto.Name.Should().Be("Lake");
            venueDto.Areas.Should().HaveCount(1);

            var waterAreaDto = venueDto.Areas[0];
            waterAreaDto.Name.Should().Be("North");

            var nestedVenueDto = waterAreaDto.Venue;
            nestedVenueDto.Should().NotBeNull();
            nestedVenueDto.Name.Should().Be("Lake");
            nestedVenueDto.Areas.Should().HaveCount(1);
        }

        [Fact]
        public void ShouldMap_WithRecursion_MapOptions()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new VenueMapping(),
                    new WaterAreaMapping(),
                },
                EnableRecursionHandling = false,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var venue = Venue.GetRecursiveVenueTestData();

            // Act
            var venueDto = mapper.Map<VenueDto>(venue, o => o.EnableRecursionHandling = true);

            // Assert
            venueDto.Should().NotBeNull();
            venueDto.Name.Should().Be("Lake");
            venueDto.Areas.Should().HaveCount(1);

            var waterAreaDto = venueDto.Areas[0];
            waterAreaDto.Name.Should().Be("North");

            var nestedVenueDto = waterAreaDto.Venue;
            nestedVenueDto.Should().NotBeNull();
            nestedVenueDto.Name.Should().Be("Lake");
            nestedVenueDto.Areas.Should().HaveCount(1);
        }

        [Fact]
        public void ShouldMap_WithRecursion_MaxDepth()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new VenueMapping(),
                    new WaterAreaMapping(),
                },
                EnableRecursionHandling = true,
                MaxDepth = 2,
            };

            IMapper mapper = new Mapper(mapperOptions);
            var venue = Venue.GetRecursiveVenueTestData();

            // Act
            var venueDto = mapper.Map<VenueDto>(venue);

            // Assert
            venueDto.Should().NotBeNull();
            venueDto.Name.Should().Be("Lake");
            venueDto.Areas.Should().HaveCount(1);

            var waterAreaDto = venueDto.Areas[0];
            waterAreaDto.Name.Should().Be("North");

            var nestedVenueDto = waterAreaDto.Venue;
            nestedVenueDto.Should().BeNull();
        }

        [Fact]
        public void ShouldMap_WithRecursion_MaxDepth_ThrowsException()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new VenueMapping(),
                    new WaterAreaMapping(),
                },
                EnableRecursionHandling = true,
                MaxDepth = 2,
                ThrowIfMaxDepthExceeded = true
            };

            IMapper mapper = new Mapper(mapperOptions);
            var venue = Venue.GetRecursiveVenueTestData();

            // Act
            Action action = () => mapper.Map<VenueDto>(venue);

            // Assert
            var ex = action.Should().Throw<MappingException>().WithInnerException<InvalidOperationException>().Which;
            ex.Message.Should().Contain("Maximum recursion depth exceeded");
        }
    }
}
