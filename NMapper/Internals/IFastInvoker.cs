namespace NMapper.Internals
{
    internal interface IFastInvoker
    {
        bool TryCreateCollectionMappingPlan(Type targetCollectionType, out IFastCollectionMappingPlan? plan);

        MappingResult Invoke(object? source, MappingContext context);
    }
}
