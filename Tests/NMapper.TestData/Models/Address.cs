using System.Diagnostics;

namespace NMapper.TestData
{
    [DebuggerDisplay("{this.City}", Type = "Address")]
    public class Address
    {
        public string? Street { get; set; }

        public string? City { get; set; }

        public int ZipCode { get; set; }
    }
}