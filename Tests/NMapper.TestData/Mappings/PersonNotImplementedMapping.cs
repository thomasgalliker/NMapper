using System;

namespace NMapper.TestData.Mappings
{
    public class PersonNotImplementedMapping : IMapping<Person, string>
    {
        public string Map(Person source)
        {
            throw new NotImplementedException();
        }
    }
}
