namespace NMapper.Tests.TestData
{
    public class PersonNotImplementedMapping : IMapping<Person, string>
    {
        public string Map(Person source)
        {
            throw new NotImplementedException();
        }
    }
}
