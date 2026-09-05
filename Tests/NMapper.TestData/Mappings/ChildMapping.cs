namespace NMapper.TestData.Mappings
{
    /// <summary>
    /// Mapping for the derived type <see cref="Child"/>. It writes the school name into
    /// <see cref="PersonDto.Address"/> so that tests can tell whether this mapping or
    /// <see cref="PersonMapping"/> was selected.
    /// </summary>
    public sealed class ChildMapping : IMapping<Child, PersonDto>
    {
        public PersonDto Map(Child source)
        {
            return new PersonDto
            {
                Id = source.Id,
                Name = source.Name,
                Address = $"School {source.SchoolName}",
            };
        }
    }
}
