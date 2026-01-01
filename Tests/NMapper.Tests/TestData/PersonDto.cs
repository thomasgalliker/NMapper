using System.Diagnostics;

namespace NMapper.Tests.TestData
{
    [DebuggerDisplay("{this.Name}", Type = "PersonDto")]
    public class PersonDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public CountryDto? Country { get; set; }
    }
}