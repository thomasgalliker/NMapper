using System;

namespace NMapper.TestData
{
    public sealed class Venue
    {
        public string Name { get; set; } = string.Empty;

        public WaterArea[] Areas { get; set; } = Array.Empty<WaterArea>();
    }

    public sealed class WaterArea
    {
        public string Name { get; set; } = string.Empty;

        public Venue Venue { get; set; } = null!;
    }

    public sealed class VenueDto
    {
        public string Name { get; set; } = string.Empty;

        public WaterAreaDto[] Areas { get; set; } = Array.Empty<WaterAreaDto>();
    }

    public sealed class WaterAreaDto
    {
        public string Name { get; set; } = string.Empty;

        public VenueDto Venue { get; set; } = null!;
    }
}
