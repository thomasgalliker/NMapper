using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using NMapper;
using BenchmarkDotNet.Attributes;
using Nelibur.ObjectMapper;
using NMapper.TestData;

namespace Benchmark
{
    public class MapperBenchmark
    {
        private readonly SourceWithCollections source = SourceWithCollectionsHelper.CreateSource(100);
        //private readonly Person[] persons = Enumerable.Range(0, 100).Select(i => new Person { Id = i }).ToArray();
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
            //TinyMapper.Bind<Person, PersonDto>();
            //TinyMapper.Bind<Person[], PersonDto[]>();
            //TinyMapper.Bind<Country, CountryDto>();
        }

        private void InitAutoMapper()
        {
            var configuration = new MapperConfiguration(x =>
            {
                x.CreateMap<SourceWithCollections, TargetWithCollections>();
                x.CreateMap<Item, ItemDto>();
                //x.CreateMap<Person, PersonDto>();
                //x.CreateMap<Country, CountryDto>();
            }, NullLoggerFactory.Instance);

            this.autoMapper = configuration.CreateMapper();
        }

        private void InitNMapper()
        {
            var mappings = new IMapping[]
            {
             new SourceToTargetCollectionsMapping(),
                new ItemMapping(),
                new ListItemMapping(),
                //new PersonMapping(),
                //new CountryMapping()
            };
            this.mapper = new NMapper.Mapper(new MapperOptions { Mappings = mappings });
        }

        [Benchmark]
        public void CollectionMapping_TinyMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = TinyMapper.Map<TargetWithCollections>(this.source);
                //var personDtos = TinyMapper.Map<PersonDto[]>(this.persons);
            }
        }

        [Benchmark]
        public void CollectionMapping_AutoMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = this.autoMapper.Map<TargetWithCollections>(this.source);
                //var personDtos = this.autoMapper.Map<PersonDto[]>(this.persons);
            }
        }

        [Benchmark]
        public void CollectionMapping_NMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = this.mapper.Map<TargetWithCollections>(this.source);
                //var personDtos = this.mapper.Map<PersonDto[]>(this.persons);
            }
        }

        [Benchmark]
        public void CollectionMapping_StaticMapping()
        {
            for (var i = 0; i < Iterations; i++)
            {
                var result = MapStatic(this.source);
                //var personDtos = MapStatic(this.persons);
            }
        }

        private static PersonDto[] MapStatic(Person[] persons)
        {
            var personDtos = new List<PersonDto>();
            foreach (var person in persons)
            {
                personDtos.Add(new PersonDto
                {
                    Id = person.Id,
                    Name = person.Name,
                    Country = new CountryDto(),
                    Address = person.Address?.Place,
                });
            }

            return personDtos.ToArray();
        }

        private static TargetWithCollections MapStatic(SourceWithCollections source)
        {
            var target = new TargetWithCollections();
            target.StringList.AddRange(source.StringList);
            source.ItemList.ForEach(x => target.ItemList.Add(HandwrittenMap(x)));
            return target;
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
    }
}
