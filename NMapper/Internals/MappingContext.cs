using System.Runtime.CompilerServices;

namespace NMapper.Internals
{
    internal sealed class MappingContext : IMappingContext
    {
        private readonly Mapper mapper;

        // Resolved once per root Map call instead of re-reading
        // options?.X ?? mapper.Options.X for every mapped object.
        internal readonly bool EnableRecursionHandling;
        internal readonly int MaxDepthValue;
        internal readonly bool ThrowIfMaxDepthExceededValue;
        internal readonly int MaxCycleVisitsValue;

        private int depth;
        private Dictionary<ReferenceKey, object>? references;
        private Dictionary<ReferenceKey, int>? inFlight;

        internal MappingContext(Mapper mapper, MapOptions? options)
        {
            this.mapper = mapper;
            this.EnableRecursionHandling = options?.EnableRecursionHandling ?? mapper.Options.EnableRecursionHandling;
            this.MaxDepthValue = options?.MaxDepth ?? mapper.Options.MaxDepth;
            this.ThrowIfMaxDepthExceededValue = options?.ThrowIfMaxDepthExceeded ?? mapper.Options.ThrowIfMaxDepthExceeded;

            var maxCycleVisits = options?.MaxCycleVisits ?? mapper.Options.MaxCycleVisits;
            this.MaxCycleVisitsValue = maxCycleVisits < 1 ? 1 : maxCycleVisits;
        }

        /// <summary>
        /// True when neither reference tracking nor depth limiting is active,
        /// i.e. when per-object bookkeeping can be skipped entirely.
        /// </summary>
        internal bool IsUntracked => !this.EnableRecursionHandling && this.MaxDepthValue <= 0;

        public TTarget? Map<TTarget>(object? source)
        {
            var sourceType = Mapper.GetSourceType(source);
            return this.Map<TTarget>(source, sourceType);
        }

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TSource, TTarget>(TSource? source)
        {
            var sourceType = Mapper.GetSourceType(source);
            return this.Map<TTarget>(source, sourceType);
        }

        private TTarget? Map<TTarget>(object? source, Type? sourceType)
        {
            return this.mapper.MapInternal<TTarget>(source, sourceType, this);
        }

        internal bool TryEnter(object? source)
        {
            var maxDepth = this.MaxDepthValue;
            if (maxDepth <= 0)
            {
                return true;
            }

            if (!ReferenceGuards.IsTrackable(source))
            {
                return true;
            }

            this.depth++;
            if (this.depth > maxDepth)
            {
                // Undo the increment: the caller skips its try/finally when TryEnter fails,
                // so Exit is never called for this frame.
                this.depth--;
                return false;
            }

            return true;
        }

        internal void Exit(object? source)
        {
            var maxDepth = this.MaxDepthValue;
            if (maxDepth <= 0)
            {
                return;
            }

            if (!ReferenceGuards.IsTrackable(source))
            {
                return;
            }

            this.depth--;
        }

        internal bool TryGetMappedObject(object? source, Type targetType, [NotNullWhen(true)] out object? target)
        {
            target = null;

            if (!this.EnableRecursionHandling ||
                source == null ||
                this.references is null ||
                !ReferenceGuards.IsTrackable(source))
            {
                return false;
            }

            return this.references.TryGetValue(new ReferenceKey(source, targetType), out target);
        }

        internal void StoreMappedObject(object? source, Type targetType, object target)
        {
            if (!this.EnableRecursionHandling ||
                source == null ||
                !ReferenceGuards.IsTrackable(source) ||
                !ReferenceGuards.IsTrackable(target))
            {
                return;
            }

            this.references ??= new Dictionary<ReferenceKey, object>();
            this.references[new ReferenceKey(source, targetType)] = target;
        }

        /// <summary>
        /// Last-resort backstop for circular references that the reference cache cannot resolve.
        /// A mapping returns a fully constructed target, so a target can only be cached once its
        /// mapping has completed. Cycles that do not route through a collection plan therefore
        /// never see a cache hit and would recurse until the stack overflows.
        /// </summary>
        /// <returns><c>false</c> once the pair has been visited <see cref="MaxCycleVisitsValue"/> times on this branch.</returns>
        internal bool TryBeginCycle(object? source, Type targetType)
        {
            if (!this.EnableRecursionHandling ||
                source is null ||
                !ReferenceGuards.IsTrackable(source))
            {
                return true;
            }

            this.inFlight ??= new Dictionary<ReferenceKey, int>();

            var key = new ReferenceKey(source, targetType);
            this.inFlight.TryGetValue(key, out var visits);
            if (visits >= this.MaxCycleVisitsValue)
            {
                return false;
            }

            this.inFlight[key] = visits + 1;
            return true;
        }

        internal void EndCycle(object? source, Type targetType)
        {
            if (!this.EnableRecursionHandling ||
                source is null ||
                this.inFlight is null ||
                !ReferenceGuards.IsTrackable(source))
            {
                return;
            }

            var key = new ReferenceKey(source, targetType);
            if (!this.inFlight.TryGetValue(key, out var visits))
            {
                return;
            }

            if (visits <= 1)
            {
                this.inFlight.Remove(key);
            }
            else
            {
                this.inFlight[key] = visits - 1;
            }
        }

        public bool ThrowIfMaxDepthExceeded => this.ThrowIfMaxDepthExceededValue;

        public int MaxDepth => this.MaxDepthValue;
    }

    /// <summary>
    /// Identifies a mapped object by its source instance <i>and</i> the requested target type.
    /// The same source may legitimately be mapped to more than one target type within one
    /// root <c>Map</c> call, so the source instance alone is not a sufficient key.
    /// </summary>
    internal readonly struct ReferenceKey : IEquatable<ReferenceKey>
    {
        private readonly object source;
        private readonly Type targetType;

        public ReferenceKey(object source, Type targetType)
        {
            this.source = source;
            this.targetType = targetType;
        }

        public bool Equals(ReferenceKey other)
        {
            return ReferenceEquals(this.source, other.source) && this.targetType == other.targetType;
        }

        public override bool Equals(object? obj)
        {
            return obj is ReferenceKey other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (RuntimeHelpers.GetHashCode(this.source) * 397) ^ this.targetType.GetHashCode();
            }
        }
    }
}
