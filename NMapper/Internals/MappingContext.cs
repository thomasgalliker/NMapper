namespace NMapper.Internals
{
    internal sealed class MappingContext : IMappingContext
    {
        private readonly Mapper mapper;
        private readonly MapOptions? options;
        private List<Exception>? exceptions;

        private int depth;
        private Dictionary<object, object>? references;

        internal MappingContext(Mapper mapper, MapOptions? options)
        {
            this.mapper = mapper;
            this.options = options;
        }

        public void AddException(Exception exception)
        {
            this.exceptions ??= new List<Exception>();

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
            if (this.exceptions == null)
            {
                return;
            }

            if (this.exceptions.Count == 1)
            {
                throw this.exceptions[0];
            }

            if (this.exceptions.Count > 1)
            {
                throw new AggregateException(this.exceptions);
            }
        }

        internal bool TryEnter(object? source)
        {
            if (!ReferenceGuards.IsTrackable(source))
            {
                return true;
            }

            var maxDepth = this.GetMaxDepth();
            if (maxDepth > 0)
            {
                this.depth++;
                if (this.depth > maxDepth)
                {
                    return false;
                }
            }

            return true;
        }

        internal void Exit(object? source)
        {
            if (!ReferenceGuards.IsTrackable(source))
            {
                return;
            }

            var maxDepth = this.GetMaxDepth();
            if (maxDepth > 0)
            {
                this.depth--;
            }
        }

        private int GetMaxDepth()
        {
            return this.options?.MaxDepth ?? this.mapper.Options.MaxDepth;
        }

        internal bool TryGetMappedObject(object? source, out object? target)
        {
            target = null;

            if (source == null ||
                !this.EnableRecursionHandling ||
                !ReferenceGuards.IsTrackable(source) ||
                this.references is null)
            {
                return false;
            }

            return this.references.TryGetValue(source, out target);
        }

        private bool EnableRecursionHandling
        {
            get => this.options?.EnableRecursionHandling ?? this.mapper.Options.EnableRecursionHandling;
        }

        public bool ThrowIfMaxDepthExceeded
        {
            get => this.options?.ThrowIfMaxDepthExceeded ?? this.mapper.Options.ThrowIfMaxDepthExceeded;
        }

        public int MaxDepth
        {
            get => this.options?.MaxDepth ?? this.mapper.Options.MaxDepth;
        }

        internal void StoreMappedObject(object? source, object target)
        {
            if (source == null ||
                !this.EnableRecursionHandling ||
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
