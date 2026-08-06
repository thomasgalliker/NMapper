namespace NMapper.TestData
{
    /// <summary>
    /// Test data for <see cref="Venue"/>.
    /// </summary>
    public static class Venues
    {
        /// <summary>
        /// Circular graph whose collection is an array.
        /// </summary>
        public static Venue CreateRecursive()
        {
            var venue = new Venue
            {
                Name = "Lake"
            };
            var area = new WaterArea
            {
                Name = "North",
                Venue = venue
            };
            venue.Areas = new[] { area };
            return venue;
        }
    }
}
