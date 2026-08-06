namespace NMapper.TestData.Mappings
{
    public sealed class CarMapping : IMappingWithContext<Car, CarDto>
    {
        public CarDto Map(Car source, IMappingContext context)
        {
            return new CarDto
            {
                Id = source.Id,
                Model = source.Model,
                Brand = context.Map<BrandDto>(source.Brand),
                Garage = context.Map<GarageDto>(source.Garage),
                Owner = context.Map<OwnerDto>(source.Owner),
            };
        }
    }
}