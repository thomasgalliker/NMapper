namespace NMapper.Internals
{
    internal interface IFastCollectionMappingPlan
    {
        object Map(object source, MappingContext context);
    }
}
