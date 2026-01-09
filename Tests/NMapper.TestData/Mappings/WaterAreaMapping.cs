namespace NMapper.TestData.Mappings
{
    public sealed class WaterAreaMapping :
        IMappingWithContext<WaterArea, WaterAreaDto>,
        IMappingWithContext<WaterAreaDto, WaterArea>
    {
        public WaterAreaDto Map(WaterArea source, IMappingContext context)
            => new WaterAreaDto()
            {
                Name = source.Name,
                Venue = context.Map<VenueDto>(source.Venue)
            };

        public WaterArea Map(WaterAreaDto source, IMappingContext context)
            => new WaterArea()
            {
                Name = source.Name,
                Venue = context.Map<Venue>(source.Venue)
            };
    }

}
