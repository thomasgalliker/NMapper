namespace NMapper.TestData.Mappings
{
    public sealed class OwnerMapping : IMappingWithContext<Owner, OwnerDto>
    {
        public OwnerDto Map(Owner source, IMappingContext context)
        {
            return new OwnerDto
            {
                Id = source.Id,
                Name = source.Name,
                Car = context.Map<CarDto>(source.Car),
            };
        }
    }
}