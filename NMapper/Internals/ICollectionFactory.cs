namespace NMapper.Internals
{
    public interface ICollectionFactory
    {
        Array CreateArray(Type elementType, int length);

        ObjectCollection CreateCollection(Type collectionType, Type elementType, int? capacity = null);
    }

    public readonly struct ObjectCollection
    {
        private readonly Action<object, object?> add;

        public ObjectCollection(object collection, Action<object, object?> add)
        {
            this.Collection = collection;
            this.add = add;
        }

        public object Collection { get; }

        public void Add(object? item)
        {
            this.add(this.Collection, item);
        }
    }
}
