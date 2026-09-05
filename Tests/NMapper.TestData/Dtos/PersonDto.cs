using System.Diagnostics;

namespace NMapper.TestData
{
    [DebuggerDisplay("{this.Name}", Type = "PersonDto")]
    public class PersonDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        /// <summary>
        /// Flattened from <see cref="Person.Address"/> by a separately registered
        /// <c>IMapping&lt;Address, string?&gt;</c>.
        /// </summary>
        public string? Address { get; set; }

        public CountryDto? Country { get; set; }

        public FamilyDto? Family { get; set; }

        public PersonDto? BestFriend { get; set; }
    }
}
