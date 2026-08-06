namespace NMapper.TestData.Mappings
{
    public sealed class BrandMapping : IMapping<Brand, BrandDto>
    {
        public BrandDto Map(Brand source)
        {
            return new BrandDto
            {
                Id = source.Id,
                Name = source.Name,
            };
        }
    }
}