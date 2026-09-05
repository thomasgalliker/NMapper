namespace NMapper.Internals
{
    internal interface IFastCollectionMappingPlan
    {
        object Map(object source, Type requestedTargetType, MappingContext context);
    }
}
