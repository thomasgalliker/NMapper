using System;

namespace NMapper.TestData
{
    public class PersonNotImplementedMapping : IMapping<Person, string>
    {
        public string Map(Person source)
        {
            throw new NotImplementedException();
        }
    }
}
