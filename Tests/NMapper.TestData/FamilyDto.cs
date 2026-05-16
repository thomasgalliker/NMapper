using System.Collections.Generic;

namespace NMapper.TestData
{
    public sealed class FamilyDto
    {
        public List<PersonDto> Members { get; set; } = new List<PersonDto>();
    }
}
