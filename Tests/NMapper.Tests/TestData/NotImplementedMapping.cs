namespace NMapper.Tests.TestData
{
    public class NotImplementedMapping : IMapping<Person, string>
    {
        public string Map(Person source)
        {
            throw new NotImplementedException();
        }
    }
}
