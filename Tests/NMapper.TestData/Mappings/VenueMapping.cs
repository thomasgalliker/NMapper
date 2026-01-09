namespace NMapper.TestData.Mappings
{
    public sealed class VenueMapping : IMappingWithContext<Venue, VenueDto>, IMappingWithContext<VenueDto, Venue>
    {
        public VenueDto Map(Venue source, IMappingContext context)
        {
            return new VenueDto()
            {
                Name = source.Name,
                Areas = context.Map<WaterAreaDto[]>(source.Areas)
            };
        }

        public Venue Map(VenueDto source, IMappingContext context)
        {
            return new Venue()
            {
                Name = source.Name,
                Areas = context.Map<WaterArea[]>(source.Areas)
            };
        }
    }
}
