using NMapper;

namespace NMapperConsoleApp
{
    public class PersonMapping : IMapping<Person, PersonDto>
    {
        public PersonDto Map(Person person)
        {
            return new PersonDto
            {
                Id = person.Id,
                Name = person.Name,
            };
        }
    }
}