using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using NMapper.Internals;

namespace NMapper
{
    public sealed class Mapper : IMapper
    {
        private readonly Dictionary<TypePair, IFastInvoker> map = new();
        private readonly ConcurrentDictionary<Type, EnumerableTypeInfo> enumerableTypeCache = new();
        private readonly ConcurrentDictionary<TypePair, CollectionMappingInfo> collectionMappingInfoCache = new();
        private readonly Dictionary<CollectionPlanKey, IFastCollectionMappingPlan> collectionMappingPlans = new();

        internal readonly MapperOptions Options;

        public Mapper()
            : this(new MapperOptions())
        {
        }

        public Mapper(params IMapping[] mappings)
            : this(new MapperOptions { Mappings = mappings })
        {
        }

        public Mapper(IEnumerable<IMapping> mappings)
            : this(new MapperOptions { Mappings = mappings.ToArray() })
        {
        }

        public Mapper(MapperOptions? options)
        {
            this.Options = options ?? new MapperOptions();
            this.RegisterMappings(this.Options.Mappings);
        }

        public void RegisterMappings(IEnumerable<IMapping> mappings)
        {
            foreach (var mapping in mappings)
            {
                this.RegisterMappingInternal(mapping, rebuildCollectionPlans: false);
            }

            this.RebuildCollectionMappingPlans();
        }

        public void RegisterMapping<TSource, TTarget>(Func<TSource, TTarget> mapping)
        {
            this.RegisterMappingInternal(new DelegateMapping<TSource, TTarget>(mapping), rebuildCollectionPlans: true);
        }

        public void RegisterMapping(IMapping mapping)
        {
            this.RegisterMappingInternal(mapping, rebuildCollectionPlans: true);
        }

        private void RegisterMappingInternal(IMapping mapping, bool rebuildCollectionPlans)
        {
            this.collectionMappingInfoCache.Clear();

            var mappingInterfaces = mapping.GetType()
                .GetInterfaces()
                .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IMapping<,>) ||
                                                i.GetGenericTypeDefinition() == typeof(IMappingWithContext<,>)))
                .ToArray();

            foreach (var mappingInterface in mappingInterfaces)
            {
                var args = mappingInterface.GetGenericArguments();
                var typePair = new TypePair(args[0], args[1]);
                if (this.map.ContainsKey(typePair))
                {
                    throw new DuplicateMappingException(typePair.SourceType, typePair.TargetType);
                }

                var useContext = mappingInterface.GetGenericTypeDefinition() == typeof(IMappingWithContext<,>);

                if (!useContext)
                {
                    var fastInvoker = FastInvoker.Create(typePair, mapping, mappingInterface);
                    this.map[typePair] = fastInvoker;
                }
                else
                {
                    var fastInvoker = FastContextInvoker.Create(typePair, mapping, mappingInterface);
                    this.map[typePair] = fastInvoker;
                }
            }

            if (rebuildCollectionPlans)
            {
                this.RebuildCollectionMappingPlans();
            }
        }

        public IEnumerable<TypePair> Mappings => this.map.Keys;

        private MappingContext CreateMappingContext(Action<MapOptions>? options)
        {
            MappingContext context;

            if (options != null)
            {
                var mapOptions = new MapOptions(this.Options);
                options(mapOptions);
                context = new MappingContext(this, mapOptions);
            }
            else
            {
                context = new MappingContext(this, null);
            }

            return context;
        }

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TTarget>(object? source)
        {
            return this.Map<TTarget>(source, null);
        }

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TTarget>(object? source, Action<MapOptions>? options)
        {
            var sourceType = GetSourceType(source);
            var context = this.CreateMappingContext(options);
            return this.MapInternal<TTarget>(source, sourceType, context);
        }

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TSource, TTarget>(TSource? source)
        {
            return this.Map<TSource, TTarget>(source, null);
        }

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TSource, TTarget>(TSource? source, Action<MapOptions>? options)
        {
            var sourceType = GetSourceType(source);
            var context = this.CreateMappingContext(options);
            return this.MapInternal<TTarget>(source, sourceType, context);
        }

        internal static Type? GetSourceType(object? source)
        {
            return source?.GetType();
        }

        internal static Type? GetSourceType<TSource>(TSource? source)
        {
            var declaredSourceType = typeof(TSource);
            var runtimeSourceType = source?.GetType();
            var sourceType = runtimeSourceType;

            if (runtimeSourceType == null)
            {
                sourceType = declaredSourceType;
            }
            else if (declaredSourceType != runtimeSourceType &&
                     declaredSourceType != typeof(object) &&
                     !declaredSourceType.IsInterface &&
                     (declaredSourceType.IsValueType || declaredSourceType.IsSealed))
            {
                sourceType = declaredSourceType;
            }

            return sourceType;
        }

        internal TTarget? MapInternal<TTarget>(object? source, Type? sourceType, MappingContext context)
        {
            if (sourceType == null)
            {
                return default;
            }

            var typePair = new TypePair(sourceType, typeof(TTarget));

            // Recursion detection
            if (!context.TryEnter(source))
            {
                if (context.ThrowIfMaxDepthExceeded)
                {
                    throw new MappingException(
                        sourceType,
                        typeof(TTarget),
                        this.GetType(),
                        new InvalidOperationException($"Maximum recursion depth exceeded (MaxDepth: {context.MaxDepth})."));
                }

                return default;
            }

            try
            {
                if (context.TryGetMappedObject(source, out var cached))
                {
                    return (TTarget?)cached;
                }

                if (this.TryExecuteExplicitMapping(source, typePair, context, out var explicitResult))
                {
                    if (explicitResult is not null)
                    {
                        context.StoreMappedObject(source, explicitResult);
                    }

                    return (TTarget?)explicitResult;
                }

                if (this.TryExecuteCompiledCollectionMappingPlan<TTarget>(source, typePair, context, out var compiledCollectionResult))
                {
                    return compiledCollectionResult;
                }

                var collectionMappingInfo = this.collectionMappingInfoCache.GetOrAdd(typePair, this.CreateCollectionMappingInfo);
                if (collectionMappingInfo.Kind == CollectionMappingKind.Array ||
                    collectionMappingInfo.Kind == CollectionMappingKind.Enumerable)
                {
                    throw new MissingMappingException(collectionMappingInfo.ElementTypePair.SourceType, collectionMappingInfo.ElementTypePair.TargetType);
                }
            }
            finally
            {
                context.Exit(source);
            }

            throw new MissingMappingException(sourceType, typePair.TargetType);
        }

        private bool TryExecuteExplicitMapping(object? source, TypePair typePair, MappingContext context, out object? result)
        {
            if (this.map.TryGetValue(typePair, out var map))
            {
                result = map.Invoke(source, context);
                return true;
            }

            result = null;
            return false;
        }

        private bool TryExecuteCompiledCollectionMappingPlan<TTarget>(object? source, TypePair typePair, MappingContext context, out TTarget? result)
        {
            var collectionMappingInfo = this.collectionMappingInfoCache.GetOrAdd(typePair, this.CreateCollectionMappingInfo);
            if (collectionMappingInfo.Kind != CollectionMappingKind.Array &&
                collectionMappingInfo.Kind != CollectionMappingKind.Enumerable)
            {
                result = default;
                return false;
            }

            var planKey = new CollectionPlanKey(collectionMappingInfo.ElementTypePair.SourceType, typePair.TargetType);
            if (!this.collectionMappingPlans.TryGetValue(planKey, out var plan))
            {
                result = default;
                return false;
            }

            result = (TTarget?)plan.Map(source!, context);
            return true;
        }

        private CollectionMappingInfo CreateCollectionMappingInfo(TypePair typePair)
        {
            return new CollectionMappingInfo(typePair, this.GetEnumerableTypeInfo);
        }

        private void RebuildCollectionMappingPlans()
        {
            this.collectionMappingPlans.Clear();

            foreach (var entry in this.map)
            {
                var sourceElementType = entry.Key.SourceType;
                var targetElementType = entry.Key.TargetType;
                var fastInvoker = entry.Value;

                foreach (var targetCollectionType in GetPrecompiledCollectionTargetTypes(targetElementType))
                {
                    if (fastInvoker.TryCreateCollectionMappingPlan(targetCollectionType, out var collectionMappingPlan))
                    {
                        this.collectionMappingPlans[new CollectionPlanKey(sourceElementType, targetCollectionType)] = collectionMappingPlan;
                    }
                }
            }
        }

        private static IEnumerable<Type> GetPrecompiledCollectionTargetTypes(Type elementType)
        {
            yield return elementType.MakeArrayType();
            yield return typeof(List<>).MakeGenericType(elementType);
            yield return typeof(IEnumerable<>).MakeGenericType(elementType);
            yield return typeof(ICollection<>).MakeGenericType(elementType);
            yield return typeof(IList<>).MakeGenericType(elementType);
            yield return typeof(IReadOnlyCollection<>).MakeGenericType(elementType);
            yield return typeof(IReadOnlyList<>).MakeGenericType(elementType);
            yield return typeof(HashSet<>).MakeGenericType(elementType);
            yield return typeof(ISet<>).MakeGenericType(elementType);
            yield return typeof(Collection<>).MakeGenericType(elementType);
        }

        private EnumerableTypeInfo GetEnumerableTypeInfo(Type type)
        {
            return this.enumerableTypeCache.GetOrAdd(type, static t => new EnumerableTypeInfo(t));
        }
    }
}
