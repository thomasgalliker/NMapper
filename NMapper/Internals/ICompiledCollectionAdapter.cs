namespace NMapper.Internals
{
    internal interface ICompiledCollectionAdapter
    {
        object Create(int? capacity);

        void Add(object collection, object? item);
    }
}
