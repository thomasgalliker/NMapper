using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using NMapper.Internals;

namespace NMapper
{
    public sealed class Mapper : IMapper
    {
        private readonly ICollectionFactory collectionFactory;
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
            this.collectionFactory = this.Options.CollectionFactory ?? new FastCollectionFactory();
            this.RegisterMappings(this.Options.Mappings);
        }

        public void RegisterMappings(IEnumerable<IMapping> mappings)
        {
            foreach (var mapping in mappings)
            {
                this.RegisterMapping(mapping);
            }

            //this.RegisterMappingInternal(new DoubleToDecimalMapping(), throwDuplicates: false);
        }

        public void RegisterMapping<TSource, TTarget>(Func<TSource, TTarget> mapping)
        {
            this.RegisterMappingInternal(new DelegateMapping<TSource, TTarget>(mapping));
        }

        public void RegisterMapping(IMapping mapping)
        {
            this.RegisterMappingInternal(mapping);
        }

        private void RegisterMappingInternal(IMapping mapping, bool throwDuplicates = true)
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

            this.RebuildCollectionMappingPlans();
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
            var sourceType = source?.GetType() ?? null;
            var context = this.CreateMappingContext(options);
            var result = this.MapInternal<TTarget>(source, sourceType, context);

            context.ThrowIfAnyException();

            return (TTarget?)result.Result;
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
            var result = this.MapInternal<TTarget>(source, sourceType, context);

            context.ThrowIfAnyException();

            return (TTarget?)result.Result;
        }

        private static Type? GetSourceType<TSource>(TSource? source)
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

        internal MappingResult MapInternal<TTarget>(object? source, Type? sourceType, MappingContext context)
        {
            if (sourceType == null)
            {
                return new MappingResult(default(TTarget), null, context);
            }

            var typePair = new TypePair(sourceType, typeof(TTarget));

            // Recursion detection
            if (!context.TryEnter(source))
            {
                MappingException? mappingException = null;

                if (context.ThrowIfMaxDepthExceeded)
                {
                    mappingException = new MappingException(
                        sourceType,
                        typeof(TTarget),
                        this.GetType(),
                        new InvalidOperationException($"Maximum recursion depth exceeded (MaxDepth: {context.MaxDepth})."));

                    context.AddException(mappingException);
                }

                return new(default, mappingException, context);
            }

            try
            {
                if (context.TryGetMappedObject(source, out var cached))
                {
                    return new MappingResult((TTarget?)cached, null, context);
                }

                if (this.TryExecuteExplicitMapping(source, typePair, context, out var explicitResult, out var explicitException))
                {
                    if (explicitException is null && explicitResult is not null)
                    {
                        context.StoreMappedObject(source, explicitResult);
                    }

                    return new MappingResult((TTarget?)explicitResult, explicitException, context);
                }

                if (this.TryExecuteCompiledCollectionMappingPlan<TTarget>(source, typePair, context, out var compiledCollectionResult))
                {
                    return compiledCollectionResult;
                }

                var collectionMappingInfo = this.collectionMappingInfoCache.GetOrAdd(typePair, this.CreateCollectionMappingInfo);

                if (collectionMappingInfo.Kind == CollectionMappingKind.Array)
                {
                    return this.MapArray<TTarget>(source, collectionMappingInfo.ElementTypePair, context);
                }

                if (collectionMappingInfo.Kind == CollectionMappingKind.Enumerable)
                {
                    return this.MapEnumerable<TTarget>(source, collectionMappingInfo.ElementTypePair, context);
                }
            }
            finally
            {
                context.Exit(source);
            }

            var missingMappingException = new MissingMappingException(sourceType, typePair.TargetType);
            context.AddException(missingMappingException);
            return new(default, missingMappingException, context);
        }

        private bool TryExecuteExplicitMapping(object? source, TypePair typePair, MappingContext context, out object? result, out Exception? exception)
        {
            if (this.map.TryGetValue(typePair, out var map))
            {
                var r = map.Invoke(source, context);
                result = r.Result;
                exception = r.Exception;
                return true;
            }

            result = null;
            exception = null;
            return false;
        }

        private bool TryExecuteCompiledCollectionMappingPlan<TTarget>(object? source, TypePair typePair, MappingContext context, out MappingResult result)
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

            var mapped = plan.Map(source!, context);
            result = new MappingResult((TTarget?)mapped, null, context);
            return true;
        }

        private MappingResult MapArray<TTarget>(object? source, TypePair elementTypePair, MappingContext context)
        {
            if (!this.map.TryGetValue(elementTypePair, out var elementMap))
            {
                var ex = new MissingMappingException(elementTypePair.SourceType, elementTypePair.TargetType);
                context.AddException(ex);

                return new MappingResult(default(TTarget), ex, context);
            }

            if (source is ICollection collection)
            {
                var array = this.collectionFactory.CreateArray(elementTypePair.TargetType, collection.Count);
                context.StoreMappedObject(source, array);

                var i = 0;
                foreach (var item in collection)
                {
                    var r = elementMap.Invoke(item, context);
                    if (r.Success)
                    {
                        array.SetValue(r.Result, i++);
                    }
                }

                return new MappingResult(array, null, context);
            }

            if (source is IEnumerable enumerable)
            {
                // fallback for IEnumerable
                var temp = new List<object?>();

                foreach (var item in enumerable)
                {
                    var r = elementMap.Invoke(item, context);
                    if (r.Success)
                    {
                        temp.Add(r.Result);
                    }
                }

                var array = this.collectionFactory.CreateArray(elementTypePair.TargetType, temp.Count);
                context.StoreMappedObject(source, array);

                for (var i = 0; i < temp.Count; i++)
                {
                    array.SetValue(temp[i], i);
                }

                return new MappingResult(array, null, context);
            }

            throw new NotSupportedException();
        }

        private MappingResult MapEnumerable<TTarget>(object? source, TypePair elementTypePair, MappingContext context)
        {
            if (!this.map.TryGetValue(elementTypePair, out var elementMap))
            {
                var ex = new MissingMappingException(elementTypePair.SourceType, elementTypePair.TargetType);
                context.AddException(ex);

                return new MappingResult(default(TTarget), ex, context);
            }

            int? capacity = source is ICollection collection ? collection.Count : null;
            var targetCollection = this.collectionFactory.CreateCollection(typeof(TTarget), elementTypePair.TargetType, capacity);
            context.StoreMappedObject(source, targetCollection.Collection);

            if (source is IEnumerable targetEnumerable)
            {
                foreach (var item in targetEnumerable)
                {
                    var mappingResult = elementMap.Invoke(item, context);
                    if (mappingResult.Success)
                    {
                        targetCollection.Add(mappingResult.Result);
                    }
                }
            }

            return new MappingResult(targetCollection.Collection, null, context);
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
                var invoker = entry.Value;

                foreach (var targetCollectionType in GetPrecompiledCollectionTargetTypes(targetElementType))
                {
                    if (invoker.TryCreateCollectionMappingPlan(targetCollectionType, out var plan))
                    {
                        this.collectionMappingPlans[new CollectionPlanKey(sourceElementType, targetCollectionType)] = plan!;
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
