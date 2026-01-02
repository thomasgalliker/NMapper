using System.Diagnostics;

namespace NMapper.Tests.TestData
{
    [DebuggerDisplay("{this.Name}", Type = "Person")]
    public class Person
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int CountryId { get; set; }

        public Address? Address { get; set; }

        public Country? Country { get; set; }
    }
}