using System.Diagnostics;

namespace NMapper.TestData
{
    [DebuggerDisplay("{this.Name}", Type = "CountryDto")]
    public class CountryDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}