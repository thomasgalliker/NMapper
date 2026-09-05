namespace NMapper.TestData
{
    /// <summary>
    /// Derived from <see cref="Person"/> so that a mapping can be resolved by the runtime type
    /// of the source rather than by its declared type.
    /// </summary>
    public sealed class Child : Person
    {
        public string SchoolName { get; set; } = null!;
    }
}
