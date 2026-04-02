using AutoMapper;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using Nelibur.ObjectMapper;
using NMapper;
using NMapper.TestData;
using System.Linq;

namespace Benchmark
{
    public class MapperBenchmark
    {
        private readonly SourceWithCollections collectionSource = SourceWithCollectionsHelper.CreateSource(100);
        private readonly Person personSource = BenchmarkTestData.CreatePerson();
        private readonly decimal? nullableDecimalSource = 10m;
        private NMapper.IMapper mapper = null!;
        private AutoMapper.IMapper autoMapper = null!;
        private const int Iterations = 1;

        public MapperBenchmark()
        {
            this.InitTinyMapper();
            this.InitAutoMapper();
            this.InitNMapper();
        }

        private void InitTinyMapper()
        {
            TinyMapper.Bind<SourceWithCollections, TargetWithCollections>();
            TinyMapper.Bind<Item, ItemDto>();
        }

        private void InitAutoMapper()
        {
            var configuration = new MapperConfiguration(x =>
            {
                x.CreateMap<SourceWithCollections, TargetWithCollections>();
                x.CreateMap<Item, ItemDto>();
                x.CreateMap<Address, string?>().ConvertUsing(source => source.Place);
                x.CreateMap<Country, CountryDto>();
                x.CreateMap<Person, PersonDto>();
                x.CreateMap<decimal?, double>().ConvertUsing(source => source.HasValue ? (double)source.Value : double.NaN);
            }, NullLoggerFactory.Instance);

            this.autoMapper = configuration.CreateMapper();
        }

        private void InitNMapper()
        {
            var mappings = new IMapping[]
            {
                new CollectionBenchmarkMapping(),
                new ItemMapping(),
                new CountryMapping(),
                new PersonMapping(),
                new DelegateMapping<Address, string?>(source => source.Place),
                new DelegateMapping<decimal?, double>(source => source.HasValue ? (double)source.Value : double.NaN),
            };

            this.mapper = new NMapper.Mapper(new MapperOptions { Mappings = mappings });
        }

        [Benchmark]
        public void CollectionMapping_TinyMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = TinyMapper.Map<TargetWithCollections>(this.collectionSource);
            }
        }

        [Benchmark]
        public void CollectionMapping_AutoMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = this.autoMapper.Map<TargetWithCollections>(this.collectionSource);
            }
        }

        [Benchmark]
        public void CollectionMapping_NMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = this.mapper.Map<TargetWithCollections>(this.collectionSource);
            }
        }

        [Benchmark]
        public void CollectionMapping_StaticMapping()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = MapStatic(this.collectionSource);
            }
        }

        [Benchmark]
        public void NestedMapping_AutoMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = this.autoMapper.Map<PersonDto>(this.personSource);
            }
        }

        [Benchmark]
        public void NestedMapping_NMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = this.mapper.Map<PersonDto>(this.personSource);
            }
        }

        [Benchmark]
        public void NestedMapping_StaticMapping()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = MapStatic(this.personSource);
            }
        }

        [Benchmark]
        public void Conversion_AutoMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = this.autoMapper.Map<double>(this.nullableDecimalSource);
            }
        }

        [Benchmark]
        public void Conversion_NMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = this.mapper.Map<decimal?, double>(this.nullableDecimalSource);
            }
        }

        [Benchmark]
        public void Conversion_StaticMapping()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = MapStatic(this.nullableDecimalSource);
            }
        }

        private static PersonDto[] MapStatic(Person[] persons)
        {
            var personDtos = new List<PersonDto>();
            foreach (var person in persons)
            {
                personDtos.Add(MapStatic(person));
            }

            return personDtos.ToArray();
        }

        private static PersonDto MapStatic(Person source)
        {
            return new PersonDto
            {
                Id = source.Id,
                Name = source.Name,
                Country = source.Country == null
                    ? null
                    : new CountryDto
                    {
                        Id = source.Country.Id,
                        Name = source.Country.Name,
                    },
                Address = source.Address?.Place,
            };
        }

        private static TargetWithCollections MapStatic(SourceWithCollections source)
        {
            var target = new TargetWithCollections();
            target.StringList.AddRange(source.StringList);
            source.ItemList.ForEach(x => target.ItemList.Add(HandwrittenMap(x)));
            return target;
        }

        private static double MapStatic(decimal? source)
        {
            return source.HasValue ? (double)source.Value : double.NaN;
        }

        private static ItemDto HandwrittenMap(Item source)
        {
            var target = new ItemDto();
            target.Id = source.Id;
            target.Bool = source.Bool;
            target.Byte = source.Byte;
            target.Char = source.Char;
            target.DateTime = source.DateTime;
            target.Decimal = source.Decimal;
            target.Float = source.Float;
            target.Int = source.Int;
            target.Long = source.Long;
            target.Short = source.Short;
            target.FirstName = source.FirstName;
            target.LastName = source.LastName;
            return target;
        }

        private sealed class CollectionBenchmarkMapping : IMappingWithContext<SourceWithCollections, TargetWithCollections>
        {
            public TargetWithCollections Map(SourceWithCollections source, IMappingContext context)
            {
                return new TargetWithCollections
                {
                    StringList = source.StringList.ToList(),
                    ItemList = context.Map<List<ItemDto>>(source.ItemList),
                };
            }
        }

        private static class BenchmarkTestData
        {
            public static Person CreatePerson()
            {
                return new Person
                {
                    Id = 42,
                    Name = "Ada Lovelace",
                    Address = new Address
                    {
                        Street = "Analytical Engine Way",
                        Place = "London",
                        ZipCode = 1000,
                    },
                    Country = new Country
                    {
                        Id = 7,
                        Name = "United Kingdom",
                        NativeName = "United Kingdom",
                    },
                };
            }
        }
    }
}
