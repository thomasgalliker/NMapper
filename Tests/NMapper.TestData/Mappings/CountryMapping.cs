namespace NMapper.TestData
{
    public class CountryMapping : IMapping<Country, CountryDto>
    {
        public CountryDto Map(Country company)
        {
            return new CountryDto
            {
                Id = company.Id,
                Name = company.Name,
            };
        }
    }
}