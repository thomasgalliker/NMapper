using System;
using System.Collections.Generic;

namespace NMapper.TestData
{
    /// <summary>
    /// Source model for the recursion tests. A garage holds cars, every car points back at its
    /// garage, and a car and its owner reference each other without any collection in between.
    /// </summary>
    public sealed class Garage
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Typed as <see cref="IEnumerable{T}"/> so tests can back it with the collection shapes
        /// that exercise the different collection mapping plans, such as <see cref="HashSet{T}"/>
        /// or a lazily evaluated sequence.
        /// </summary>
        public IEnumerable<Car> Cars { get; set; } = Array.Empty<Car>();

        /// <summary>
        /// A single car mapped before <see cref="Cars"/>, so that a truncated branch and the
        /// siblings following it can be observed within one mapping.
        /// </summary>
        public Car? CourtesyCar { get; set; }
    }
}
