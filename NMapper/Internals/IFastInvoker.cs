namespace NMapper.Internals
{
    internal interface IFastInvoker
    {
        bool TryCreateCollectionMappingPlan(Type targetCollectionType, [NotNullWhen(true)] out IFastCollectionMappingPlan? plan);

        object? Invoke(object? source, MappingContext context);
    }
}
