using System.Diagnostics;

namespace NMapperConsoleApp
{
    [DebuggerDisplay("{this.Name}", Type = "Person")]
    public class Person
    {
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}