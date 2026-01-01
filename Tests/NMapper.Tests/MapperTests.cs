using FluentAssertions;
using NMapper.Tests.TestData;
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
            var personsCount = 10;
            var persons = Enumerable.Range(1, personsCount)
                .Select(i => new Person
                {
                    Id = i,
                    Name = $"Person {i}",
                    CountryId = country.Id,
                    Country = country,
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
        public void ShouldMap_ThrowsMappingException()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new NotImplementedMapping(),
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

    }
}
