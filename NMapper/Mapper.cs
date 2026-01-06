using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NMapper.Internals;

namespace NMapper
{
    public sealed class Mapper : IMapper
    {
        private readonly Dictionary<TypePair, IFastInvoker> map = new();

        private readonly ILogger<Mapper> logger;

        public Mapper()
            : this(new NullLogger<Mapper>())
        {
        }

        public Mapper(params IMapping[] mappings)
            : this((IEnumerable<IMapping>)mappings)
        {
        }

        public Mapper(IEnumerable<IMapping> mappings)
            : this(new NullLogger<Mapper>(), mappings)
        {
        }

        public Mapper(ILogger<Mapper> logger)
        {
            this.logger = logger;
        }

        public Mapper(ILogger<Mapper> logger, params IMapping[] mappings)
            : this(logger, (IEnumerable<IMapping>)mappings)
        {
        }

        public Mapper(ILogger<Mapper> logger, IEnumerable<IMapping> mappings)
        {
            this.logger = logger;
            this.RegisterMappings(mappings);
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

                var method = mappingInterface.GetMethod("Map")!;

                var useContext = mappingInterface.GetGenericTypeDefinition() == typeof(IMappingWithContext<,>);

                if (!useContext)
                {
                    var delType = typeof(Func<,>).MakeGenericType(args[0], args[1]);

                    var del = Delegate.CreateDelegate(delType, mapping, method);

                    var invokerType = typeof(FastInvoker<,>).MakeGenericType(args[0], args[1]);

                    var invoker = (IFastInvoker)Activator.CreateInstance(invokerType, del, typePair, mapping.GetType())!;

                    this.map[typePair] = invoker;
                }
                else
                {
                    var delType = typeof(Func<,,>).MakeGenericType(
                          args[0],
                          typeof(MappingContext),
                          args[1]);

                    var del = Delegate.CreateDelegate(delType, mapping, method);

                    var invokerType = typeof(FastContextInvoker<,>).MakeGenericType(args[0], args[1]);

                    var invoker = (IFastInvoker)Activator.CreateInstance(invokerType, del, typePair, mapping.GetType())!;

                    this.map[typePair] = invoker;
                }
            }
        }

        public IEnumerable<TypePair> Mappings => this.map.Keys;

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TTarget>(object? source)
        {
            var sourceType = source?.GetType() ?? null;
            var context = new MappingContext(this);
            var result = this.MapInternal<TTarget>(source, sourceType, context);

            context.ThrowIfAny();

            return (TTarget?)result.Result;
        }

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TSource, TTarget>(TSource? source)
        {
            var sourceType = typeof(TSource);

            var context = new MappingContext(this);
            var result = this.MapInternal<TTarget>(source, sourceType, context);

            context.ThrowIfAny();

            return (TTarget?)result.Result;
        }

        internal MappingResult MapInternal<TTarget>(object? source, Type? sourceType, MappingContext context)
        {
            if (sourceType == null)
            {
                return default;
            }

            var typePair = new TypePair(sourceType, typeof(TTarget));

            if (this.TryExecuteExplicitMapping(source, typePair, context, out var explicitResult, out var explicitException))
            {
                return new MappingResult((TTarget?)explicitResult, explicitException, context);
            }

            if (TryGetEnumerableElementType(sourceType, out var sourceElementType))
            {
                // Array
                if (typePair.TargetType.IsArray)
                {
                    var targetElementType = typePair.TargetType.GetElementType()!;
                    var mappingResult = this.MapArray<TTarget>(source, new TypePair(sourceElementType, targetElementType), context);
                    return mappingResult;
                }

                // IEnumerable<T>
                {
                    if (TryGetEnumerableElementType(typePair.TargetType, out var targetElementType))
                    {
                        var mappingResult = this.MapEnumerable<TTarget>(source, new TypePair(sourceElementType, targetElementType), context);
                        return mappingResult;
                    }
                }
            }

            var ex = new MissingMappingException(sourceType, typePair.TargetType);
            context.AddException(ex);
            return new(default, ex, context);
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
                var array = Array.CreateInstance(elementTypePair.TargetType, collection.Count);

                int i = 0;
                foreach (var item in collection)
                {
                    var r = elementMap.Invoke(item, context);
                    if (r.Exception == null)
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
                    if (r.Exception == null)
                    {
                        temp.Add(r.Result);
                    }
                }

                var array = Array.CreateInstance(elementTypePair.TargetType, temp.Count);
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
            var listType = typeof(List<>).MakeGenericType(elementTypePair.TargetType);

            if (!this.map.TryGetValue(elementTypePair, out var elementMap))
            {
                var ex = new MissingMappingException(elementTypePair.SourceType, elementTypePair.TargetType);
                context.AddException(ex);

                return new MappingResult(default(TTarget), ex, context);
            }

            var list = (IList)Activator.CreateInstance(listType)!;

            if (source is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    var mappingResult = elementMap.Invoke(item, context);
                    if (mappingResult.Exception == null)
                    {
                        list.Add(mappingResult.Result);
                    }
                }
            }

            return new MappingResult(list, null, context);
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