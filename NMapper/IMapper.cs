using System.Diagnostics.CodeAnalysis;

namespace NMapper
{
    public interface IMapper
    {
        void RegisterMapping(IMapping mapping);

        void RegisterMapping<TSource, TTarget>(Func<TSource, TTarget> mapping);

        void RegisterMappings(IEnumerable<IMapping> mappings);

        IEnumerable<(Type SourceType, Type TargetType)> Mappings { get; }

        [return: NotNullIfNotNull(nameof(source))]
        TTarget? Map<TTarget>(object? source);

        [return: NotNullIfNotNull(nameof(source))]
        TTarget? Map<TSource, TTarget>(TSource? source);
    }

    public interface IMappingContext
    {
        [return: NotNullIfNotNull(nameof(source))]
        TTarget? Map<TTarget>(object? source);

        [return: NotNullIfNotNull(nameof(source))]
        TTarget? Map<TSource, TTarget>(TSource? source);
    }
}
