using System.Diagnostics;

namespace NMapper.TestData
{
    [DebuggerDisplay("{this.Name}", Type = "Country")]
    public class Country
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? NativeName { get; set; }
    }
}