using System.Linq;

namespace NMapper.TestData
{
    public sealed class FamilyMapping : IMappingWithContext<Family, FamilyDto>
    {
        public FamilyDto Map(Family source, IMappingContext context)
        {
            return new FamilyDto
            {
                Members = source.Members
                    .Select(member => context.Map<Person, PersonDto>(member))
                    .ToList()!,
            };
        }
    }
}
