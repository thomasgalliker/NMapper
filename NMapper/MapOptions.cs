namespace NMapper
{
    public class MapOptions
    {
        /// <summary>
        /// Enables tracking of object references during mapping.
        /// When enabled, previously mapped source objects will be reused
        /// to preserve object identity and correctly handle circular graphs.
        /// </summary>
        /// <remarks>
        /// Enabling this option has a measurable runtime cost.
        /// </remarks>
        public bool EnableRecursionHandling { get; set; } = false;

        /// <summary>
        /// Limits the maximum depth of object graph traversal during mapping.
        /// This can be used to prevent infinite recursion for self-referential types.
        /// Default: 0 (disabled).
        /// </summary>
        public int MaxDepth { get; set; } = 0;

        /// <summary>
        /// Throws a <see cref="MappingException"/> if a recursive mapping exceeds the configured <see cref="MaxDepth"/>.
        /// </summary>
        public bool ThrowIfMaxDepthExceeded { get; set; } = false;
    }
}