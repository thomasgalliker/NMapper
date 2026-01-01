using System.Diagnostics;

namespace NMapper.Tests.TestData
{
    [DebuggerDisplay("{this.Name}", Type = "CountryDto")]
    public class CountryDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}