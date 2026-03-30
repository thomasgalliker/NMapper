namespace NMapper
{
    public interface IMappingContext
    {
        [return: NotNullIfNotNull(nameof(source))]
        TTarget? Map<TTarget>(object? source);

        [return: NotNullIfNotNull(nameof(source))]
        TTarget? Map<TSource, TTarget>(TSource? source);
    }
}
