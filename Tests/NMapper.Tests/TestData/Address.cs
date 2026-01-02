using System.Diagnostics;

namespace NMapper.Tests.TestData
{
    [DebuggerDisplay("{this.Name}", Type = "Address")]
    public class Address
    {
        public string? Street { get; set; }

        public string? Place { get; set; }

        public int ZipCode { get; set; }
    }
}