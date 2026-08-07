namespace NMapper.TestData
{
    /// <summary>
    /// Second target type for <see cref="Person"/>, so that one source can be mapped to more
    /// than one target type within a single root mapping call.
    /// </summary>
    public sealed class PersonSummaryDto
    {
        public string Text { get; set; } = string.Empty;
    }
}
