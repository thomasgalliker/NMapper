namespace NMapper.TestData
{
    /// <summary>
    /// Test data for <see cref="Family"/>.
    /// </summary>
    public static class Families
    {
        /// <summary>
        /// Circular graph whose collection is an array: the family holds one member and that
        /// member points back at the family.
        /// </summary>
        public static Family CreateRecursive()
        {
            var family = new Family
            {
                Name = "Miller"
            };
            var member = new Person
            {
                Id = 1,
                Name = "John",
                Family = family
            };
            family.Members = new[] { member };
            return family;
        }
    }
}
