namespace NMapper.TestData.Mappings
{
    public sealed class GarageMapping : IMappingWithContext<Garage, GarageDto>
    {
        public GarageDto Map(Garage source, IMappingContext context)
        {
            return new GarageDto
            {
                Name = source.Name,
                CourtesyCar = context.Map<CarDto>(source.CourtesyCar),
                Cars = context.Map<CarDto[]>(source.Cars),
                CarSummaries = context.Map<CarSummaryDto[]>(source.Cars),
            };
        }
    }
}
