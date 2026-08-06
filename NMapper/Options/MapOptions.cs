namespace NMapper
{
    public sealed class MapOptions
    {
        internal MapOptions(MapperOptions options)
        {
            this.EnableRecursionHandling = options.EnableRecursionHandling;
            this.MaxDepth = options.MaxDepth;
            this.ThrowIfMaxDepthExceeded = options.ThrowIfMaxDepthExceeded;
            this.MaxCycleVisits = options.MaxCycleVisits;
        }

        /// <summary>
        /// Enables tracking of object references during mapping.
        /// When enabled, a source object that is mapped more than once to the same target type
        /// within a single root map call is mapped only once and the result is reused,
        /// which allows circular object graphs to terminate.
        /// </summary>
        /// <remarks>
        /// Enabling this option has a measurable runtime cost.
        /// <para>
        /// A mapping returns a fully constructed target, so a target can only be tracked once its
        /// mapping has completed. A cycle that runs purely through single references therefore
        /// cannot be closed by the reference cache; such cycles are cut once the same source and
        /// target type has been visited <see cref="MaxCycleVisits"/> times on one branch of the
        /// graph, which means the mapped result may contain more than one target instance for the
        /// same source. Cycles that run through a collection are not affected, because the
        /// collection is created by the mapper and is tracked before its elements are mapped.
        /// </para>
        /// </remarks>
        public bool EnableRecursionHandling { get; set; } = false;

        /// <summary>
        /// Limits the maximum depth of object graph traversal during mapping.
        /// This can be used to prevent infinite recursion for self-referential types.
        /// Default: 0 (disabled).
        /// </summary>
        /// <remarks>
        /// Only mapped objects count towards the depth, including the elements of a collection.
        /// A collection itself is a container rather than a level of the object graph and does
        /// not consume depth.
        /// </remarks>
        public int MaxDepth { get; set; } = 0;

        /// <summary>
        /// Throws a <see cref="MappingException"/> if a recursive mapping exceeds the configured
        /// <see cref="MaxDepth"/>, or if an unresolvable circular reference is cut.
        /// </summary>
        public bool ThrowIfMaxDepthExceeded { get; set; } = false;

        /// <summary>
        /// How often the same source object and target type may appear on a single branch of the
        /// object graph before an unresolvable circular reference is cut.
        /// Only applies when <see cref="EnableRecursionHandling"/> is enabled.
        /// Default: 2.
        /// </summary>
        /// <remarks>
        /// A circular reference that runs through a collection resolves on its second visit,
        /// because the mapper creates the target collection itself and tracks it before mapping
        /// its elements. Values below 2 therefore also cut collection-mediated cycles, which would
        /// otherwise have completed. Values below 1 are treated as 1.
        /// </remarks>
        public int MaxCycleVisits { get; set; } = 2;
    }
}