using NMapper.Internals;

namespace NMapper
{
    public class MapperOptions
    {
        private static readonly ICollectionFactory DefaultCollectionFactory = new FastCollectionFactory();

        public ICollectionFactory CollectionFactory { get; set; } = DefaultCollectionFactory;
    }
}