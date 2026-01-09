using System.Diagnostics.CodeAnalysis;

namespace NMapper.Internals
{
    internal sealed class MappingContext : IMappingContext
    {
        private readonly Mapper mapper;
        private readonly List<Exception> exceptions = new();

        private int depth;
        private Dictionary<object, object>? references;

        internal MappingContext(Mapper mapper)
        {
            this.mapper = mapper;
        }

        public void AddException(Exception exception)
        {
            if (!this.exceptions.Contains(exception))
            {
                this.exceptions.Add(exception);
            }
        }

        public TTarget? Map<TTarget>(object? source)
        {
            var sourceType = source?.GetType() ?? null;
            return this.Map<TTarget>(source, sourceType);
        }

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TSource, TTarget>(TSource? source)
        {
            var sourceType = typeof(TSource);
            return this.Map<TTarget>(source, sourceType);
        }

        private TTarget? Map<TTarget>(object? source, Type? sourceType)
        {
            var result = this.mapper.MapInternal<TTarget>(source, sourceType, this);
            if (result.Exception != null)
            {
                this.AddException(result.Exception);
                return default;
            }
            else
            {
                return (TTarget?)result.Result;
            }
        }

        public void ThrowIfAnyException()
        {
            if (this.exceptions.Count == 1)
            {
                throw this.exceptions[0];
            }

            if (this.exceptions.Count > 1)
            {
                throw new AggregateException(this.exceptions);
            }
        }

        internal bool TryEnter(object source)
        {
            if (!ReferenceGuards.IsTrackable(source))
            {
                return true;
            }

            if (this.mapper.Options.MaxDepth > 0)
            {
                this.depth++;
                if (this.depth > this.mapper.Options.MaxDepth)
                {
                    return false;
                }
            }

            return true;
        }

        internal void Exit(object source)
        {
            if (!ReferenceGuards.IsTrackable(source))
            {
                return;
            }

            if (this.mapper.Options.MaxDepth > 0)
            {
                this.depth--;
            }
        }

        internal bool TryGetMappedObject(object source, out object? target)
        {
            target = null;

            if (!this.mapper.Options.EnableRecursionHandling ||
                !ReferenceGuards.IsTrackable(source) ||
                this.references is null)
            {
                return false;
            }

            return this.references.TryGetValue(source, out target);
        }

        internal void StoreMappedObject(object source, object target)
        {
            if (!this.mapper.Options.EnableRecursionHandling ||
                !ReferenceGuards.IsTrackable(source) ||
                !ReferenceGuards.IsTrackable(target))
            {
                return;
            }

            this.references ??= new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            this.references[source] = target;
        }

    }
}