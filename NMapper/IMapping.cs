namespace NMapper
{
    /// <summary>
    /// Marker interface for mapping implementations.
    /// Used for discovery and registration only.
    /// </summary>
    public interface IMapping
    {
    }

    /// <summary>
    /// Defines a mapping from <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
    /// Implementations must be stateless and must not depend on the global mapper.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TTarget">Target type.</typeparam>
    public interface IMapping<in TSource, out TTarget> : IMapping
    {
        /// <summary>
        /// Maps the source object to the target object.
        /// </summary>
        TTarget Map(TSource source);
    }

    /// <summary>
    /// Defines a mapping that requires a mapping context to invoke other mappings.
    /// Use only when nested or composed mappings are needed.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TTarget">Target type.</typeparam>
    public interface IMappingWithContext<in TSource, out TTarget> : IMapping
    {
        /// <summary>
        /// Maps the source object to the target object using the provided context.
        /// </summary>
        TTarget Map(TSource source, IMappingContext context);
    }
}
