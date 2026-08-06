using System;

namespace NMapper.TestData
{
    public sealed class GarageDto
    {
        public string Name { get; set; } = string.Empty;

        public CarDto? CourtesyCar { get; set; }

        /// <summary>
        /// Mapped by a context-aware element mapping, which closes the cycle back to the garage.
        /// </summary>
        public CarDto[] Cars { get; set; } = Array.Empty<CarDto>();

        /// <summary>
        /// The same source collection mapped to a second target type by a plain
        /// <see cref="IMapping{TSource,TTarget}"/>, so the context-free collection plan is
        /// exercised alongside the context-aware one.
        /// </summary>
        public CarSummaryDto[] CarSummaries { get; set; } = Array.Empty<CarSummaryDto>();
    }

    public sealed class CarDto
    {
        public int Id { get; set; }

        public string Model { get; set; } = string.Empty;

        public BrandDto? Brand { get; set; }

        public GarageDto? Garage { get; set; }

        public OwnerDto? Owner { get; set; }
    }

    public sealed class BrandDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Second target type for <see cref="Car"/>.
    /// </summary>
    public sealed class CarSummaryDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public sealed class OwnerDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public CarDto? Car { get; set; }
    }
}
