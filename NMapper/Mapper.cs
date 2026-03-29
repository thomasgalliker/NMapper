using System.Collections;
using System.Diagnostics.CodeAnalysis;
using NMapper.Internals;

namespace NMapper
{
    public sealed class Mapper : IMapper
    {
        private readonly ICollectionFactory collectionFactory;
        private readonly Dictionary<TypePair, IFastInvoker> map = new();
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


                if (TryGetEnumerableElementType(sourceType, out var sourceElementType))
                {
                    // Array
                    if (typePair.TargetType.IsArray)
                    {
                        var targetElementType = typePair.TargetType.GetElementType()!;
                        var elementTypePair = new TypePair(sourceElementType, targetElementType);
                        var mappingResult = this.MapArray<TTarget>(source, elementTypePair, context);
                        return mappingResult;
                    }

                    // IEnumerable<T>
                    {
                        if (TryGetEnumerableElementType(typePair.TargetType, out var targetElementType))
                        {
                            var elementTypePair = new TypePair(sourceElementType, targetElementType);
                            var mappingResult = this.MapEnumerable<TTarget>(source, elementTypePair, context);
                            return mappingResult;
                        }
                    }
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
            else if (source is IEnumerable enumerable)
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

        private static bool TryGetEnumerableElementType(Type type, out Type elementType)
        {
            // string is IEnumerable<char>, but we never want to treat it as a collection
            if (type == typeof(string))
            {
                elementType = null!;
                return false;
            }

            // Array
            if (type.IsArray)
            {
                elementType = type.GetElementType()!;
                return true;
            }

            // IEnumerable<T> itself
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            // Implementations of IEnumerable<T>
            var enumerableInterface = type.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
            {
                elementType = enumerableInterface.GetGenericArguments()[0];
                return true;
            }

            elementType = null!;
            return false;
        }
    }
}
