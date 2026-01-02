using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NMapper.Tests.Logging;
using NMapper.Tests.TestData;
using Xunit;
using Xunit.Abstractions;

namespace NMapper.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public ServiceCollectionExtensionsTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ShouldResolveMappings_FromMappingAssembly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMapping(o =>
            {
                o.MappingAssembly = this.GetType().Assembly;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            var person = new Person
            {
                Id = 1,
                Name = "John"
            };

            var personDto = mapper.Map<PersonDto>(person);

            // Arrange
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Country) && x.TargetType == typeof(CountryDto));
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Person) && x.TargetType == typeof(PersonDto));

            personDto.Should().NotBeNull();
            personDto.Id.Should().Be(person.Id);
            personDto.Name.Should().Be(person.Name);
        }

        [Fact]
        public void ShouldResolveMappings_FromMappingAssemblies()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddProvider(new TestOutputHelperLoggerProvider(this.testOutputHelper));
            });
            services.AddMapping(o =>
            {
                o.MappingAssemblies = new[]
                {
                    this.GetType().Assembly
                };
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            var person = new Person
            {
                Id = 1,
                Name = "John",
                CountryId = 2,
                Country = new Country
                {
                    Id = 2,
                    Name = "USA",
                    NativeName = "United States of America",
                }
            };

            var personDto = mapper.Map<PersonDto>(person);

            // Arrange
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Country) && x.TargetType == typeof(CountryDto));
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Person) && x.TargetType == typeof(PersonDto));

            personDto.Should().NotBeNull();
            personDto.Id.Should().Be(person.Id);
            personDto.Name.Should().Be(person.Name);
            personDto.Country.Should().BeEquivalentTo(new CountryDto
            {
                Id = 2,
                Name = "USA",
            });
        }

        [Fact]
        public void ShouldThrowDuplicateMappingException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMapping(options =>
            {
                options.MappingAssemblies = new[]
                {
                    this.GetType().Assembly,
                    this.GetType().Assembly,
                };
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            Action action = () => serviceProvider.GetRequiredService<IMapper>();

            // Arrange
            action.Should().Throw<DuplicateMappingException>();
        }

        [Fact]
        public void ShouldGetMappingsFromConfiguration_AddMappings()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMapping(o =>
            {
                o.Mappings.Add(new IMapping[]
                {
                    new PersonMapping(),
                    new CountryMapping(),
                });
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Arrange
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Country) && x.TargetType == typeof(CountryDto));
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Person) && x.TargetType == typeof(PersonDto));
        }

        [Fact]
        public void ShouldGetMappingsFromConfiguration_AddMappingTypes()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMapping(o =>
            {
                o.Mappings.Add(new Type[]
                {
                    typeof(PersonMapping),
                    typeof(CountryMapping)
                });
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var mapper = serviceProvider.GetRequiredService<IMapper>();

            // Arrange
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Country) && x.TargetType == typeof(CountryDto));
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Person) && x.TargetType == typeof(PersonDto));
        }
    }
}