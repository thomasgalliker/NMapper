using System;
using System.Collections.Generic;

namespace NMapper.TestData
{
    /// <summary>
    /// Root of the test domain. A family holds people, every person points back at their family,
    /// and two people can name each other as best friend without any collection in between.
    /// </summary>
    public sealed class Family
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// A single person mapped before <see cref="Members"/>, so that a truncated branch and the
        /// siblings following it can be observed within one mapping.
        /// </summary>
        public Person? Head { get; set; }

        /// <summary>
        /// Typed as <see cref="IEnumerable{T}"/> so tests can back it with the collection shapes
        /// that exercise the different collection mapping plans, such as <see cref="HashSet{T}"/>
        /// or a lazily evaluated sequence.
        /// </summary>
        public IEnumerable<Person> Members { get; set; } = Array.Empty<Person>();
    }
}
