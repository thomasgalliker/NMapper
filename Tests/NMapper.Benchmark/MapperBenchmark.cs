using NMapper;
using BenchmarkDotNet.Attributes;
using Nelibur.ObjectMapper;
using AMapper = AutoMapper.Mapper;
using NMapper.TestData;

namespace Benchmark
{
    public class MapperBenchmark
    {
        private readonly SourceWithCollections source = SourceWithCollectionsHelper.CreateSource(100);
        private IMapper mapper = null!;
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
        }

        private void InitAutoMapper()
        {
            AMapper.Initialize(x =>
            {
                x.CreateMap<SourceWithCollections, TargetWithCollections>();
                x.CreateMap<Item, Item>();
            });
        }

        private void InitNMapper()
        {
            this.mapper = new Mapper(new SourceToTargetCollectionsMapping(), new ItemMapping());
        }

        [Benchmark]
        public void CollectionMapping_TinyMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                TinyMapper.Map<TargetWithCollections>(this.source);
            }
        }

        [Benchmark]
        public void CollectionMapping_AutoMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                AMapper.Map<TargetWithCollections>(this.source);
            }
        }

        [Benchmark]
        public void CollectionMapping_NMapper()
        {
            for (var i = 0; i < Iterations; i++)
            {
                this.mapper.Map<TargetWithCollections>(this.source);
            }
        }

        [Benchmark]
        public void CollectionMapping_StaticMapping()
        {
            for (var i = 0; i < Iterations; i++)
            {
                MapStatic(this.source, new TargetWithCollections());
            }
        }

        private static TargetWithCollections MapStatic(SourceWithCollections source, TargetWithCollections target)
        {
            target.StringList.AddRange(source.StringList);
            source.ItemList.ForEach(x => target.ItemList.Add(HandwrittenMap(x, new Item())));
            return target;
        }

        private static Item HandwrittenMap(Item source, Item target)
        {
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
