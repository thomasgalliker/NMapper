namespace NMapper.TestData.Mappings
{
    /// <summary>
    /// Deliberately a plain <see cref="IMapping{TSource,TTarget}"/> so that collections of
    /// <see cref="Person"/> also exercise the context-free collection mapping plans.
    /// </summary>
    public sealed class PersonSummaryMapping : IMapping<Person, PersonSummaryDto>
    {
        public PersonSummaryDto Map(Person source)
        {
            return new PersonSummaryDto
            {
                Text = $"<{source.Name}>",
            };
        }
    }
}
