using System.Diagnostics;

namespace NMapperConsoleApp
{
    [DebuggerDisplay("{this.Name}", Type = "PersonDto")]
    public class PersonDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}