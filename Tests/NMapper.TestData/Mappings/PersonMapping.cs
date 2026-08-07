namespace NMapper.TestData.Mappings
{
    /// <summary>
    /// Two-way mapping between <see cref="Person"/> and <see cref="PersonDto"/>.
    /// </summary>
    /// <remarks>
    /// The member order in <see cref="Map(Person, IMappingContext)"/> is significant:
    /// <see cref="PersonDto.Address"/> is resolved first, so a missing
    /// <c>IMapping&lt;Address, string?&gt;</c> is the first failure a test observes.
    /// <para>
    /// The back-references are mapped through the <c>Map&lt;TTarget&gt;(object?)</c> overload,
    /// which returns <c>null</c> for a <c>null</c> source before it looks up a mapping. Tests that
    /// do not care about the family graph can therefore leave those references unset without
    /// having to register <see cref="FamilyMapping"/>.
    /// </para>
    /// </remarks>
    public class PersonMapping :
        IMappingWithContext<Person, PersonDto>,
        IMapping<PersonDto, Person>
    {
        public PersonDto Map(Person person, IMappingContext context)
        {
            return new PersonDto
            {
                Id = person.Id,
                Name = person.Name,
                Address = context.Map<string?>(person.Address),
                Country = context.Map<CountryDto?>(person.Country),
                Family = context.Map<FamilyDto?>(person.Family),
                BestFriend = context.Map<PersonDto?>(person.BestFriend),
            };
        }

        public Person Map(PersonDto personDto)
        {
            return new Person
            {
                Id = personDto.Id,
                Name = personDto.Name,
            };
        }
    }
}
