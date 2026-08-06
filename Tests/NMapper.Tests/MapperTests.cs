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
        public void ShouldMap_GenericOverload_UsesRuntimeTypeForDerivedSource()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new PersonMapping(),
                new EmployeeMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            Person person = new Employee
            {
                Name = "Jane Doe",
                EmployeeNumber = "E-100",
            };

            // Act
            var personDto = mapper.Map<Person, PersonDto>(person);

            // Assert
            personDto.Should().NotBeNull();
            personDto.Name.Should().Be("Jane Doe");
            personDto.Address.Should().Contain("E-100");
        }

        [Fact]
        public void ShouldMap_GenericOverload_UsesRuntimeTypeForInterfaceSource()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new EmployeeMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            IIdentifiable employee = new Employee
            {
                Name = "John Doe",
                EmployeeNumber = "E-200",
            };

            // Act
            var personDto = mapper.Map<IIdentifiable, PersonDto>(employee);

            // Assert
            personDto.Should().NotBeNull();
            personDto.Name.Should().Be("John Doe");
            personDto.Address.Should().Contain("E-200");
        }

        [Fact]
        public void ShouldMap_ContextGenericOverload_UsesRuntimeTypeForDerivedSource()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new FamilyMapping(),
                new PersonMapping(),
                new EmployeeMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var family = new Family
            {
                Members =
                {
                    new Employee
                    {
                        Name = "Jane Doe",
                        EmployeeNumber = "E-300",
                    },
                },
            };

            // Act
            var familyDto = mapper.Map<FamilyDto>(family);

            // Assert
            familyDto.Should().NotBeNull();
            familyDto.Members.Should().ContainSingle();
            familyDto.Members[0].Name.Should().Be("Jane Doe");
            familyDto.Members[0].Address.Should().Contain("E-300");
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
            var ex = action.Should().Throw<MissingMappingException>().Which;
            ex.Message.Should().Contain("No mapping registered for Address to String");
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
            var ex = action.Should().Throw<MissingMappingException>().Which;
            ex.Message.Should().Contain("No mapping registered for Person to Int32");
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
        public void ShouldMapCollections_ArrayToEnumerable()
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
            var personDtos = mapper.Map<IEnumerable<PersonDto>>(persons);

            // Assert
            personDtos.Should().NotBeNull();
            personDtos.Should().HaveCount(3);
            personDtos.All(p => p.Name?.StartsWith("Person") == true).Should().BeTrue();
        }

        [Fact]
        public void ShouldMapCollections_ListToList()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new CountryMapping(),
                new PersonMapping(),
                new DelegateMapping<Address, string?>(source => source.Place),
            };
            IMapper mapper = new Mapper(mappings);

            var persons = new List<Person>
            {
                new()
                {
                    Id = 1,
                    Name = "Person 1",
                    Address = new Address
                    {
                        Place = "Bern",
                    },
                    Country = new Country
                    {
                        Id = 10,
                        Name = "Switzerland",
                    },
                },
                new()
                {
                    Id = 2,
                    Name = "Person 2",
                    Address = new Address
                    {
                        Place = "Zurich",
                    },
                    Country = new Country
                    {
                        Id = 20,
                        Name = "Germany",
                    },
                },
            };

            // Act
            var personDtos = mapper.Map<List<PersonDto>>(persons);

            // Assert
            personDtos.Should().NotBeNull();
            personDtos.Should().HaveCount(2);
            personDtos[0].Address.Should().Be("Bern");
            personDtos[0].Country!.Name.Should().Be("Switzerland");
            personDtos[1].Address.Should().Be("Zurich");
            personDtos[1].Country!.Id.Should().Be(20);
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
        public void ShouldMapCollections_ListToCollection()
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
            var personDtos = mapper.Map<Collection<PersonDto>>(persons);

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
        public void ShouldMapCollections_ListToReadOnlyList()
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
            var personDtos = mapper.Map<IReadOnlyList<PersonDto>>(persons);

            // Assert
            personDtos.Should().NotBeNull();
            personDtos.Should().HaveCount(3);
            personDtos.All(p => p.Name?.StartsWith("Person") == true).Should().BeTrue();
        }

        [Fact]
        public void ShouldMapCollections_ListToReadOnlyCollection()
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
            var personDtos = mapper.Map<IReadOnlyCollection<PersonDto>>(persons);

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
            var ex = action.Should().Throw<MissingMappingException>().Which;
            ex.Message.Should().Contain("No mapping registered for Person to Int32");
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

            var venue = Venues.CreateRecursive();

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

            var venue = Venues.CreateRecursive();

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

            var venue = Venues.CreateRecursive();

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
            var venue = Venues.CreateRecursive();

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
            var venue = Venues.CreateRecursive();

            // Act
            Action action = () => mapper.Map<VenueDto>(venue);

            // Assert
            var ex = action.Should().Throw<MappingException>().WithInnerException<InvalidOperationException>().Which;
            ex.Message.Should().Contain("Maximum recursion depth exceeded");
        }

        [Fact]
        public void ShouldMap_WithRecursion_HashSetSourceCollectionToArray()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new GarageMapping(),
                    new CarMapping(),
                    new CarSummaryMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var garage = new Garage { Name = "Downtown" };
            var car = new Car { Id = 1, Model = "Model S", Garage = garage };

            // HashSet is deliberate: unlike List, Queue or SortedSet it does not implement the
            // non-generic ICollection, so the array plan cannot read its count and has to buffer
            // it. Replacing this with an array or a list would map a different code path.
            garage.Cars = new HashSet<Car> { car };

            // Act
            var garageDto = mapper.Map<GarageDto>(garage);

            // Assert
            garageDto.Should().NotBeNull();
            garageDto.Cars.Should().HaveCount(1);

            var carDto = garageDto.Cars[0];
            carDto.Model.Should().Be("Model S");

            var nestedGarageDto = carDto.Garage;
            nestedGarageDto.Should().NotBeNull();
            nestedGarageDto!.Name.Should().Be("Downtown");

            // The cycle must close against the very same target collection,
            // which is only possible if the plan registered it before filling it.
            nestedGarageDto.Cars.Should().BeSameAs(garageDto.Cars);

            // Same source collection mapped by a context-free element mapping.
            garageDto.CarSummaries.Should().HaveCount(1);
            garageDto.CarSummaries[0].Text.Should().Be("<Model S>");
            nestedGarageDto.CarSummaries.Should().BeSameAs(garageDto.CarSummaries);
        }

        [Fact]
        public void ShouldMap_WithRecursion_LazyEnumerableSourceCollectionToArray()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new GarageMapping(),
                    new CarMapping(),
                    new CarSummaryMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var garage = new Garage { Name = "Downtown" };
            var car = new Car { Id = 1, Model = "Model S", Garage = garage };

            // Select keeps the sequence lazy: it is neither an array nor an ICollection, so its
            // length is unknown up front and the array plan has to buffer it before it can
            // register the target array. An array or a list would map a different code path.
            garage.Cars = new[] { car }.Select(c => c);

            // Act
            var garageDto = mapper.Map<GarageDto>(garage);

            // Assert
            garageDto.Should().NotBeNull();
            garageDto.Cars.Should().HaveCount(1);
            garageDto.Cars[0].Garage!.Cars.Should().BeSameAs(garageDto.Cars);
        }

        [Fact]
        public void ShouldMap_WithRecursion_SharedCollectionElement_PreservesIdentity()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new GarageMapping(),
                    new CarMapping(),
                    new CarSummaryMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var garage = new Garage { Name = "Downtown" };
            var car = new Car { Id = 1, Model = "Model S", Garage = garage };
            garage.Cars = new[] { car, car };

            // Act
            var garageDto = mapper.Map<GarageDto>(garage);

            // Assert
            garageDto.Cars.Should().HaveCount(2);
            garageDto.Cars[0].Should().BeSameAs(garageDto.Cars[1]);

            // Identity must hold on the context-free element path as well.
            garageDto.CarSummaries.Should().HaveCount(2);
            garageDto.CarSummaries[0].Should().BeSameAs(garageDto.CarSummaries[1]);
        }

        [Fact]
        public void ShouldMap_WithRecursion_SameSourceToDifferentTargetTypes()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new GarageMapping(),
                    new CarMapping(),
                    new CarSummaryMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var garage = new Garage { Name = "Downtown" };
            var car = new Car { Id = 1, Model = "Model S", Garage = garage };
            garage.Cars = new[] { car };

            // Act
            var garageDto = mapper.Map<GarageDto>(garage);

            // Assert
            // The same source collection, and the same element within it, are mapped to two
            // different target types within one root map call.
            garageDto.Cars.Should().HaveCount(1);
            garageDto.Cars[0].Model.Should().Be("Model S");
            garageDto.CarSummaries.Should().HaveCount(1);
            garageDto.CarSummaries[0].Text.Should().Be("<Model S>");
        }

        [Fact]
        public void ShouldMap_WithRecursion_CycleWithoutCollection()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new OwnerMapping(),
                    new CarMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var owner = new Owner { Id = 1, Name = "Jane Doe" };
            owner.Car = new Car { Id = 1, Model = "Model S", Owner = owner };

            // Act
            var ownerDto = mapper.Map<OwnerDto>(owner);

            // Assert
            ownerDto.Should().NotBeNull();
            ownerDto.Name.Should().Be("Jane Doe");

            // No collection plan can register a target here, so the cycle is cut
            // after the source/target pair has been visited MaxCycleVisits times.
            ownerDto.Car.Should().NotBeNull();
            ownerDto.Car!.Model.Should().Be("Model S");
            ownerDto.Car.Owner.Should().NotBeNull();
            ownerDto.Car.Owner!.Name.Should().Be("Jane Doe");
            ownerDto.Car.Owner.Car.Should().NotBeNull();
            ownerDto.Car.Owner.Car!.Owner.Should().BeNull();
        }

        [Fact]
        public void ShouldMap_WithRecursion_CycleWithoutCollection_ThrowsException()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new OwnerMapping(),
                    new CarMapping(),
                },
                EnableRecursionHandling = true,
                ThrowIfMaxDepthExceeded = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var owner = new Owner { Id = 1, Name = "Jane Doe" };
            owner.Car = new Car { Id = 1, Model = "Model S", Owner = owner };

            // Act
            Action action = () => mapper.Map<OwnerDto>(owner);

            // Assert
            var ex = action.Should().Throw<MappingException>().WithInnerException<InvalidOperationException>().Which;
            ex.Message.Should().Contain("Unresolvable circular reference detected");
        }

        [Fact]
        public void ShouldMap_WithRecursion_MaxCycleVisits_CutsCycleEarlier()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new OwnerMapping(),
                    new CarMapping(),
                },
                EnableRecursionHandling = true,
                MaxCycleVisits = 1,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var owner = new Owner { Id = 1, Name = "Jane Doe" };
            owner.Car = new Car { Id = 1, Model = "Model S", Owner = owner };

            // Act
            var ownerDto = mapper.Map<OwnerDto>(owner);

            // Assert
            // With a single permitted visit the cycle is cut one level earlier than the default.
            ownerDto.Name.Should().Be("Jane Doe");
            ownerDto.Car.Should().NotBeNull();
            ownerDto.Car!.Model.Should().Be("Model S");
            ownerDto.Car.Owner.Should().BeNull();
        }

        [Fact]
        public void ShouldMap_WithRecursion_MaxCycleVisits_BelowOneIsTreatedAsOne()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new OwnerMapping(),
                    new CarMapping(),
                },
                EnableRecursionHandling = true,
                MaxCycleVisits = 0,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var owner = new Owner { Id = 1, Name = "Jane Doe" };
            owner.Car = new Car { Id = 1, Model = "Model S", Owner = owner };

            // Act
            var ownerDto = mapper.Map<OwnerDto>(owner);

            // Assert
            // A value below 1 must not cut the root mapping itself.
            ownerDto.Should().NotBeNull();
            ownerDto.Name.Should().Be("Jane Doe");
            ownerDto.Car!.Owner.Should().BeNull();
        }

        [Fact]
        public void ShouldMap_WithRecursion_MaxDepth_TruncatedCollectionElement_ThrowsException()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new GarageMapping(),
                    new CarMapping(),
                    new CarSummaryMapping(),
                },
                EnableRecursionHandling = true,
                MaxDepth = 1,
                ThrowIfMaxDepthExceeded = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var garage = new Garage { Name = "Downtown" };
            var car = new Car { Id = 1, Model = "Model S", Garage = garage };
            garage.Cars = new[] { car };

            // Act
            Action action = () => mapper.Map<GarageDto>(garage);

            // Assert
            // The depth limit is reached on a collection element, which must report the same way
            // as any other mapped object rather than silently yielding a null element.
            var ex = action.Should().Throw<MappingException>().WithInnerException<InvalidOperationException>().Which;
            ex.Message.Should().Contain("Maximum recursion depth exceeded");
        }

        [Fact]
        public void ShouldMap_WithRecursion_MaxDepth_DoesNotTruncateSiblings()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new GarageMapping(),
                    new CarMapping(),
                    new BrandMapping(),
                    new CarSummaryMapping(),
                },
                MaxDepth = 2,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var garage = new Garage { Name = "Downtown" };
            garage.CourtesyCar = new Car
            {
                Id = 1,
                Model = "Model S",
                Brand = new Brand { Id = 1, Name = "Tesla" },
                Garage = garage,
            };
            garage.Cars = new[]
            {
                new Car { Id = 2, Model = "Model 3", Garage = garage },
                new Car { Id = 3, Model = "Model X", Garage = garage },
            };

            // Act
            var garageDto = mapper.Map<GarageDto>(garage);

            // Assert
            // The courtesy car and the cars all sit at the same depth, so truncating the courtesy
            // car's branches must not consume the budget of the cars mapped after it.
            garageDto.CourtesyCar.Should().NotBeNull();
            garageDto.CourtesyCar!.Model.Should().Be("Model S");
            garageDto.CourtesyCar.Brand.Should().BeNull();
            garageDto.CourtesyCar.Garage.Should().BeNull();

            garageDto.Cars.Should().HaveCount(2);
            garageDto.Cars[0].Should().NotBeNull();
            garageDto.Cars[0].Model.Should().Be("Model 3");
            garageDto.Cars[0].Garage.Should().BeNull();
            garageDto.Cars[1].Should().NotBeNull();
            garageDto.Cars[1].Model.Should().Be("Model X");
            garageDto.Cars[1].Garage.Should().BeNull();
        }
    }
}
