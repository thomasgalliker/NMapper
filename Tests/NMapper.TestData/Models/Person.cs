using System.Diagnostics;

namespace NMapper.TestData
{
    /// <summary>
    /// Central model of the test domain: a person who may belong to a <see cref="TestData.Family"/>.
    /// </summary>
    /// <remarks>
    /// Deliberately not <c>sealed</c>. The mapper falls back to the declared source type for sealed
    /// types, so sealing this class would silently bypass the mapping registered for
    /// <see cref="Child"/> whenever the source is typed as <see cref="Person"/>.
    /// It also deliberately does not override <c>Equals</c>/<c>GetHashCode</c>, so that
    /// <see cref="System.Collections.Generic.HashSet{T}"/> fixtures keep reference semantics.
    /// </remarks>
    [DebuggerDisplay("{this.Name}", Type = "Person")]
    public class Person : IIdentifiable
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        /// <summary>
        /// Flattened into a single string by the target model.
        /// </summary>
        public Address? Address { get; set; }

        /// <summary>
        /// Plain nested reference without a back-reference, so the graph has non-cyclic depth too.
        /// </summary>
        public Country? Country { get; set; }

        /// <summary>
        /// Back-reference closing the <see cref="TestData.Family"/> to <see cref="Person"/> cycle,
        /// which runs through a collection.
        /// </summary>
        public Family? Family { get; set; }

        /// <summary>
        /// Back-reference closing a cycle that runs purely through single references:
        /// two people name each other as their best friend.
        /// </summary>
        public Person? BestFriend { get; set; }
    }
}
