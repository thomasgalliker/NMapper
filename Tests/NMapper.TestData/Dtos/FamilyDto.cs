using System;

namespace NMapper.TestData
{
    public sealed class FamilyDto
    {
        public string Name { get; set; } = string.Empty;

        public PersonDto? Head { get; set; }

        /// <summary>
        /// Mapped by a context-aware element mapping, which closes the cycle back to the family.
        /// Declared as an array so that source sequences of unknown length have to be buffered
        /// before the target collection can be registered with the mapping context.
        /// </summary>
        public PersonDto[] Members { get; set; } = Array.Empty<PersonDto>();

        /// <summary>
        /// The same source collection mapped to a second target type by a plain
        /// <see cref="IMapping{TSource,TTarget}"/>, so the context-free collection plan is
        /// exercised alongside the context-aware one.
        /// </summary>
        public PersonSummaryDto[] MemberSummaries { get; set; } = Array.Empty<PersonSummaryDto>();
    }
}
