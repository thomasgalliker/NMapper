namespace NMapper.TestData.Mappings
{
    /// <summary>
    /// Maps the root of the test domain.
    /// </summary>
    /// <remarks>
    /// The member order is significant. <see cref="FamilyDto.Head"/> is mapped before
    /// <see cref="FamilyDto.Members"/>, so that a truncated branch and the siblings following it
    /// can be told apart. <see cref="FamilyDto.Members"/> is mapped before
    /// <see cref="FamilyDto.MemberSummaries"/>, so that the context-aware collection is the one
    /// registered first with the mapping context.
    /// <para>
    /// <see cref="FamilyDto.Head"/> uses the <c>Map&lt;TSource, TTarget&gt;</c> overload, which
    /// resolves the mapping from the runtime type of the source. Unlike
    /// <c>Map&lt;TTarget&gt;(object?)</c>, that overload falls back to the declared type for a
    /// <c>null</c> source and would invoke the mapping with <c>null</c>, hence the explicit guard.
    /// </para>
    /// </remarks>
    public sealed class FamilyMapping : IMappingWithContext<Family, FamilyDto>
    {
        public FamilyDto Map(Family source, IMappingContext context)
        {
            return new FamilyDto
            {
                Name = source.Name,
                Head = source.Head is null ? null : context.Map<Person, PersonDto>(source.Head),
                Members = context.Map<PersonDto[]>(source.Members),
                MemberSummaries = context.Map<PersonSummaryDto[]>(source.Members),
            };
        }
    }
}
