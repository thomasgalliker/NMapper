namespace NMapper.TestData
{
    public class PersonMapping : IMappingWithContext<Person, PersonDto> //, IMapping<PersonDto, Person>
    {
        public PersonDto Map(Person person, IMappingContext context)
        {
            return new PersonDto
            {
                Id = person.Id,
                Name = person.Name,
                Address = context.Map<string?>(person.Address),
                Country = context.Map<CountryDto?>(person.Country),
            };
        }

        public Person Map(PersonDto personDto)
        {
            return new Person
            {
                Id = personDto.Id,
                Name = personDto.Name,
                CountryId = 0,
                Country = null,
            };
        }
    }

    //public class PersonMapping :
    //    IMapping<Person, PersonDto>,
    //    IMapping<PersonDto, Person>
    //{
    //    public PersonDto Map(Person person) => new PersonDto
    //    {
    //        Id = person.Id,
    //        Name = person.Name,
    //    };

    //    public Person Map(PersonDto personDto) => new Person
    //    {
    //        Id = personDto.Id,
    //        Name = personDto.Name,
    //    };
    //}
}