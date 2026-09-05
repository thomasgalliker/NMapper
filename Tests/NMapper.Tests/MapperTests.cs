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
            // PersonMapping maps in both directions, so registering it registers two type pairs.
            mapper.Mappings.Should().HaveCount(2);
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(Person) && x.TargetType == typeof(PersonDto));
            mapper.Mappings.Should().Contain(x => x.SourceType == typeof(PersonDto) && x.TargetType == typeof(Person));
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
        public void ShouldMap_PersonDtoToPerson_UsesReverseDirectionOfSameMapping()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new PersonMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var personDto = new PersonDto
            {
                Id = 1,
                Name = "John Doe",
            };

            // Act
            var person = mapper.Map<Person>(personDto);

            // Assert
            // PersonMapping declares both directions, and both are registered from the one instance.
            person.Should().NotBeNull();
            person.Id.Should().Be(personDto.Id);
            person.Name.Should().Be(personDto.Name);
        }

        [Fact]
        public void ShouldMap_GenericOverload_UsesRuntimeTypeForDerivedSource()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new PersonMapping(),
                new ChildMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            Person person = new Child
            {
                Name = "Jane Doe",
                SchoolName = "Sunnyside",
            };

            // Act
            var personDto = mapper.Map<Person, PersonDto>(person);

            // Assert
            personDto.Should().NotBeNull();
            personDto.Name.Should().Be("Jane Doe");
            personDto.Address.Should().Contain("Sunnyside");
        }

        [Fact]
        public void ShouldMap_GenericOverload_UsesRuntimeTypeForInterfaceSource()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new ChildMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            IIdentifiable child = new Child
            {
                Name = "John Doe",
                SchoolName = "Riverside",
            };

            // Act
            var personDto = mapper.Map<IIdentifiable, PersonDto>(child);

            // Assert
            personDto.Should().NotBeNull();
            personDto.Name.Should().Be("John Doe");
            personDto.Address.Should().Contain("Riverside");
        }

        [Fact]
        public void ShouldMap_ContextGenericOverload_UsesRuntimeTypeForDerivedSource()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new FamilyMapping(),
                new PersonMapping(),
                new PersonSummaryMapping(),
                new ChildMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var family = new Family
            {
                Head = new Child
                {
                    Name = "Jane Doe",
                    SchoolName = "Hillcrest",
                },
            };

            // Act
            var familyDto = mapper.Map<FamilyDto>(family);

            // Assert
            familyDto.Should().NotBeNull();
            familyDto.Head.Should().NotBeNull();
            familyDto.Head!.Name.Should().Be("Jane Doe");
            familyDto.Head.Address.Should().Contain("Hillcrest");
        }

        [Fact]
        public void ShouldMap_CollectionElement_UsesDeclaredElementTypeForDerivedSource()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new FamilyMapping(),
                new PersonMapping(),
                new PersonSummaryMapping(),
                new ChildMapping(),
            };
            IMapper mapper = new Mapper(mappings);

            var child = new Child
            {
                Name = "Jane Doe",
                SchoolName = "Hillcrest",
            };
            var family = new Family
            {
                Head = child,
                Members = new Person[] { child },
            };

            // Act
            var familyDto = mapper.Map<FamilyDto>(family);

            // Assert
            // A collection mapping plan is bound to one concrete element mapping when it is built,
            // so it does not dispatch on the runtime type of an element. The very same child is
            // mapped by ChildMapping as the head of the family, but by PersonMapping as a member.
            familyDto.Head!.Address.Should().Contain("Hillcrest");

            familyDto.Members.Should().ContainSingle();
            familyDto.Members[0].Name.Should().Be("Jane Doe");
            familyDto.Members[0].Address.Should().BeNull();
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
                    Country = country,
                    Address = new Address
                    {
                        Street = "123 Main St",
                        City = "Toronto",
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
                new DelegateMapping<Address, string?>(source => source.City),
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
                        City = "Bern",
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
                        City = "Zurich",
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
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                }
            };

            IMapper mapper = new Mapper(mapperOptions);

            var family = Families.CreateRecursive();

            // Act
            mapper.Map<FamilyDto>(family);

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
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var family = Families.CreateRecursive();

            // Act
            var familyDto = mapper.Map<FamilyDto>(family);

            // Assert
            familyDto.Should().NotBeNull();
            familyDto.Name.Should().Be("Miller");
            familyDto.Members.Should().HaveCount(1);

            var memberDto = familyDto.Members[0];
            memberDto.Name.Should().Be("John");

            var nestedFamilyDto = memberDto.Family;
            nestedFamilyDto.Should().NotBeNull();
            nestedFamilyDto!.Name.Should().Be("Miller");
            nestedFamilyDto.Members.Should().HaveCount(1);
        }

        [Fact]
        public void ShouldMap_WithRecursion_MapOptions()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                },
                EnableRecursionHandling = false,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var family = Families.CreateRecursive();

            // Act
            var familyDto = mapper.Map<FamilyDto>(family, o => o.EnableRecursionHandling = true);

            // Assert
            familyDto.Should().NotBeNull();
            familyDto.Name.Should().Be("Miller");
            familyDto.Members.Should().HaveCount(1);

            var memberDto = familyDto.Members[0];
            memberDto.Name.Should().Be("John");

            var nestedFamilyDto = memberDto.Family;
            nestedFamilyDto.Should().NotBeNull();
            nestedFamilyDto!.Name.Should().Be("Miller");
            nestedFamilyDto.Members.Should().HaveCount(1);
        }

        [Fact]
        public void ShouldMap_WithRecursion_MaxDepth()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                },
                EnableRecursionHandling = true,
                MaxDepth = 2,
            };

            IMapper mapper = new Mapper(mapperOptions);
            var family = Families.CreateRecursive();

            // Act
            var familyDto = mapper.Map<FamilyDto>(family);

            // Assert
            // The family is the first level and its members are the second. A collection is a
            // container rather than a level of the graph, so it does not consume any budget.
            familyDto.Should().NotBeNull();
            familyDto.Name.Should().Be("Miller");
            familyDto.Members.Should().HaveCount(1);

            var memberDto = familyDto.Members[0];
            memberDto.Name.Should().Be("John");
            memberDto.Family.Should().BeNull();
        }

        [Fact]
        public void ShouldMap_WithRecursion_MaxDepth_ThrowsException()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                },
                EnableRecursionHandling = true,
                MaxDepth = 2,
                ThrowIfMaxDepthExceeded = true
            };

            IMapper mapper = new Mapper(mapperOptions);
            var family = Families.CreateRecursive();

            // Act
            Action action = () => mapper.Map<FamilyDto>(family);

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
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var family = new Family { Name = "Miller" };
            var person = new Person { Id = 1, Name = "John", Family = family };

            // HashSet is deliberate: unlike List, Queue or SortedSet it does not implement the
            // non-generic ICollection, so the array plan cannot read its count and has to buffer
            // it. Replacing this with an array or a list would map a different code path.
            family.Members = new HashSet<Person> { person };

            // Act
            var familyDto = mapper.Map<FamilyDto>(family);

            // Assert
            familyDto.Should().NotBeNull();
            familyDto.Members.Should().HaveCount(1);

            var memberDto = familyDto.Members[0];
            memberDto.Name.Should().Be("John");

            var nestedFamilyDto = memberDto.Family;
            nestedFamilyDto.Should().NotBeNull();
            nestedFamilyDto!.Name.Should().Be("Miller");

            // The cycle must close against the very same target collection,
            // which is only possible if the plan registered it before filling it.
            nestedFamilyDto.Members.Should().BeSameAs(familyDto.Members);

            // Same source collection mapped by a context-free element mapping.
            familyDto.MemberSummaries.Should().HaveCount(1);
            familyDto.MemberSummaries[0].Text.Should().Be("<John>");
            nestedFamilyDto.MemberSummaries.Should().BeSameAs(familyDto.MemberSummaries);
        }

        [Fact]
        public void ShouldMap_WithRecursion_LazyEnumerableSourceCollectionToArray()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var family = new Family { Name = "Miller" };
            var person = new Person { Id = 1, Name = "John", Family = family };

            // Select keeps the sequence lazy: it is neither an array nor an ICollection, so its
            // length is unknown up front and the array plan has to buffer it before it can
            // register the target array. An array or a list would map a different code path.
            family.Members = new[] { person }.Select(p => p);

            // Act
            var familyDto = mapper.Map<FamilyDto>(family);

            // Assert
            familyDto.Should().NotBeNull();
            familyDto.Members.Should().HaveCount(1);
            familyDto.Members[0].Family!.Members.Should().BeSameAs(familyDto.Members);
        }

        [Fact]
        public void ShouldMap_WithRecursion_SharedCollectionElement_PreservesIdentity()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var family = new Family { Name = "Miller" };
            var person = new Person { Id = 1, Name = "John", Family = family };
            family.Members = new[] { person, person };

            // Act
            var familyDto = mapper.Map<FamilyDto>(family);

            // Assert
            familyDto.Members.Should().HaveCount(2);
            familyDto.Members[0].Should().BeSameAs(familyDto.Members[1]);

            // Identity must hold on the context-free element path as well.
            familyDto.MemberSummaries.Should().HaveCount(2);
            familyDto.MemberSummaries[0].Should().BeSameAs(familyDto.MemberSummaries[1]);
        }

        [Fact]
        public void ShouldMap_WithRecursion_SameSourceToDifferentTargetTypes()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var family = new Family { Name = "Miller" };
            var person = new Person { Id = 1, Name = "John", Family = family };
            family.Members = new[] { person };

            // Act
            var familyDto = mapper.Map<FamilyDto>(family);

            // Assert
            // The same source collection, and the same element within it, are mapped to two
            // different target types within one root map call.
            familyDto.Members.Should().HaveCount(1);
            familyDto.Members[0].Name.Should().Be("John");
            familyDto.MemberSummaries.Should().HaveCount(1);
            familyDto.MemberSummaries[0].Text.Should().Be("<John>");
        }

        [Fact]
        public void ShouldMap_WithRecursion_CycleWithoutCollection()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new PersonMapping(),
                },
                EnableRecursionHandling = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var alice = new Person { Id = 1, Name = "Alice" };
            var bob = new Person { Id = 2, Name = "Bob", BestFriend = alice };
            alice.BestFriend = bob;

            // Act
            var aliceDto = mapper.Map<PersonDto>(alice);

            // Assert
            aliceDto.Should().NotBeNull();
            aliceDto.Name.Should().Be("Alice");

            // No collection plan can register a target here, so the cycle is cut
            // after the source/target pair has been visited MaxCycleVisits times.
            aliceDto.BestFriend.Should().NotBeNull();
            aliceDto.BestFriend!.Name.Should().Be("Bob");
            aliceDto.BestFriend.BestFriend.Should().NotBeNull();
            aliceDto.BestFriend.BestFriend!.Name.Should().Be("Alice");
            aliceDto.BestFriend.BestFriend.BestFriend.Should().NotBeNull();
            aliceDto.BestFriend.BestFriend.BestFriend!.Name.Should().Be("Bob");
            aliceDto.BestFriend.BestFriend.BestFriend.BestFriend.Should().BeNull();
        }

        [Fact]
        public void ShouldMap_WithRecursion_CycleWithoutCollection_ThrowsException()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new PersonMapping(),
                },
                EnableRecursionHandling = true,
                ThrowIfMaxDepthExceeded = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var alice = new Person { Id = 1, Name = "Alice" };
            var bob = new Person { Id = 2, Name = "Bob", BestFriend = alice };
            alice.BestFriend = bob;

            // Act
            Action action = () => mapper.Map<PersonDto>(alice);

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
                    new PersonMapping(),
                },
                EnableRecursionHandling = true,
                MaxCycleVisits = 1,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var alice = new Person { Id = 1, Name = "Alice" };
            var bob = new Person { Id = 2, Name = "Bob", BestFriend = alice };
            alice.BestFriend = bob;

            // Act
            var aliceDto = mapper.Map<PersonDto>(alice);

            // Assert
            // With a single permitted visit the cycle is cut one level earlier than the default.
            aliceDto.Name.Should().Be("Alice");
            aliceDto.BestFriend.Should().NotBeNull();
            aliceDto.BestFriend!.Name.Should().Be("Bob");
            aliceDto.BestFriend.BestFriend.Should().BeNull();
        }

        [Fact]
        public void ShouldMap_WithRecursion_MaxCycleVisits_BelowOneIsTreatedAsOne()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new PersonMapping(),
                },
                EnableRecursionHandling = true,
                MaxCycleVisits = 0,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var alice = new Person { Id = 1, Name = "Alice" };
            var bob = new Person { Id = 2, Name = "Bob", BestFriend = alice };
            alice.BestFriend = bob;

            // Act
            var aliceDto = mapper.Map<PersonDto>(alice);

            // Assert
            // A value below 1 must not cut the root mapping itself.
            aliceDto.Should().NotBeNull();
            aliceDto.Name.Should().Be("Alice");
            aliceDto.BestFriend!.BestFriend.Should().BeNull();
        }

        [Fact]
        public void ShouldMap_WithRecursion_MaxDepth_TruncatedCollectionElement_ThrowsException()
        {
            // Arrange
            var mapperOptions = new MapperOptions
            {
                Mappings = new IMapping[]
                {
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                },
                EnableRecursionHandling = true,
                MaxDepth = 1,
                ThrowIfMaxDepthExceeded = true,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var family = new Family { Name = "Miller" };
            var person = new Person { Id = 1, Name = "John", Family = family };
            family.Members = new[] { person };

            // Act
            Action action = () => mapper.Map<FamilyDto>(family);

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
                    new FamilyMapping(),
                    new PersonMapping(),
                    new PersonSummaryMapping(),
                    new CountryMapping(),
                },
                MaxDepth = 2,
            };

            IMapper mapper = new Mapper(mapperOptions);

            var family = new Family { Name = "Miller" };
            family.Head = new Person
            {
                Id = 1,
                Name = "Anna",
                Country = new Country { Id = 1, Name = "Switzerland" },
                Family = family,
            };
            family.Members = new[]
            {
                new Person { Id = 2, Name = "Ben", Family = family },
                new Person { Id = 3, Name = "Clara", Family = family },
            };

            // Act
            var familyDto = mapper.Map<FamilyDto>(family);

            // Assert
            // The head of the family and its members all sit at the same depth, so truncating the
            // head's branches must not consume the budget of the members mapped after it.
            familyDto.Head.Should().NotBeNull();
            familyDto.Head!.Name.Should().Be("Anna");
            familyDto.Head.Country.Should().BeNull();
            familyDto.Head.Family.Should().BeNull();

            familyDto.Members.Should().HaveCount(2);
            familyDto.Members[0].Should().NotBeNull();
            familyDto.Members[0].Name.Should().Be("Ben");
            familyDto.Members[0].Family.Should().BeNull();
            familyDto.Members[1].Should().NotBeNull();
            familyDto.Members[1].Name.Should().Be("Clara");
            familyDto.Members[1].Family.Should().BeNull();
        }
    }
}
