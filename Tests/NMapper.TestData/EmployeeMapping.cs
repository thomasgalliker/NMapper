namespace NMapper.TestData
{
    public class EmployeeMapping : IMapping<Employee, PersonDto>
    {
        public PersonDto Map(Employee source)
        {
            return new PersonDto
            {
                Id = source.Id,
                Name = source.Name,
                Address = $"Employee {source.EmployeeNumber}",
            };
        }
    }
}