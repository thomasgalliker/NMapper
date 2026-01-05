using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NMapper.Internals;

namespace NMapper
{
    public sealed class Mapper : IMapper
    {
        private readonly Dictionary<TypePair, Func<object?, MappingContext, MappingResult>> map = new();

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

                Func<object?, MappingContext, object?> compiled;

                if (!useContext)
                {
                    var methodInfo = mappingInterface.GetMethod(nameof(IMapping<object, object>.Map))!;
                    compiled = this.CompileMapping(mapping, typePair, methodInfo);
                }
                else
                {
                    var methodInfo = mappingInterface.GetMethod(nameof(IMappingWithContext<object, object>.Map))!;
                    compiled = this.CompileMappingWithContext(mapping, typePair, methodInfo);
                }

                this.map[typePair] = (source, context) => this.Map(typePair, mapping, compiled, source, context);
            }
        }

        private Func<object?, MappingContext, object?> CompileMapping(IMapping mapping, TypePair typePair, MethodInfo method)
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var contextParam = Expression.Parameter(typeof(MappingContext), "context");

            var castSource = Expression.Convert(sourceParam, typePair.SourceType);

            var call = Expression.Call(Expression.Constant(mapping), method, castSource);

            var castResult = Expression.Convert(call, typeof(object));

            var expr = Expression.Lambda<Func<object?, MappingContext, object?>>(castResult, sourceParam, contextParam);
            return expr.Compile();
        }

        private Func<object?, MappingContext, object?> CompileMappingWithContext(IMapping mapping, TypePair typePair, MethodInfo method)
        {
            var sourceParam = Expression.Parameter(typeof(object), "source");
            var contextParam = Expression.Parameter(typeof(MappingContext), "context");

            var castSource = Expression.Convert(sourceParam, typePair.SourceType);

            var call = Expression.Call(Expression.Constant(mapping), method, castSource, contextParam);

            var castResult = Expression.Convert(call, typeof(object));

            var expr = Expression.Lambda<Func<object?, MappingContext, object?>>(castResult, sourceParam, contextParam);
            return expr.Compile();
        }

        private MappingResult Map(TypePair typepair, IMapping mapping, Func<object?, MappingContext, object?> map, object? source, MappingContext context)
        {
            try
            {
                var result = map(source, context);
                return new MappingResult(result, null, context);
            }
            catch (TargetInvocationException ex)
            {
                var innerException = ex.InnerException != null ? ex.InnerException : ex;
                var mappingException = new MappingException(typepair.SourceType, typepair.TargetType, mapping.GetType(), innerException);
                context.AddException(mappingException);
                return new MappingResult(default, mappingException, context);
            }
            catch (Exception ex)
            {
                var mappingException = new MappingException(typepair.SourceType, typepair.TargetType, mapping.GetType(), ex);
                context.AddException(mappingException);
                return new MappingResult(default, mappingException, context);
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
                    var array = this.MapArray(source, new TypePair(sourceElementType, targetElementType), context);
                    return new((TTarget?)array, null, context);
                }

                // IEnumerable<T>
                {
                    if (TryGetEnumerableElementType(typePair.TargetType, out var targetElementType))
                    {
                        var enumerable = this.MapEnumerable(source, new TypePair(sourceElementType, targetElementType), context);
                        return new((TTarget?)enumerable, null, context);
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
                var r = map(source, context);
                result = r.Result;
                exception = r.Exception;
                return true;
            }

            result = null;
            exception = null;
            return false;
        }

        private MappingResult ExecuteMapping(object? source, TypePair typePair, MappingContext context)
        {
            if (typePair == null)
            {
                return new MappingResult(null, null, context);
            }

            if (!this.map.TryGetValue(typePair, out var map))
            {
                var ex = new MissingMappingException(typePair.SourceType, typePair.TargetType);
                context.AddException(ex);
                return new MappingResult(null, ex, context);
            }

            try
            {
                var result = map(source, context);
                return new MappingResult(result.Result, result.Exception, context);
            }
            catch (Exception ex)
            {
                return new MappingResult(null, ex, context);
            }
        }

        private object MapArray(object? source, TypePair elementTypePair, MappingContext context)
        {
            Array? array;

            if (source is ICollection collection)
            {
                array = Array.CreateInstance(elementTypePair.TargetType, collection.Count);
                int i = 0;

                foreach (var item in collection)
                {
                    var result = this.ExecuteMapping(item, elementTypePair, context);
                    if (result.Exception == null)
                    {
                        array.SetValue(result.Result, i++);
                    }
                }
            }
            else if (source is IEnumerable enumerable)
            {
                var list = new List<object?>();

                foreach (var item in enumerable)
                {
                    var result = this.ExecuteMapping(item, elementTypePair, context);
                    if (result.Exception == null)
                    {
                        list.Add(result.Result);
                    }
                }

                array = Array.CreateInstance(elementTypePair.TargetType, list.Count);
                for (var i = 0; i < list.Count; i++)
                {
                    array.SetValue(list[i], i);
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            return array;
        }

        private object MapEnumerable(object? source, TypePair elementTypePair, MappingContext context)
        {
            var listType = typeof(List<>).MakeGenericType(elementTypePair.TargetType);
            var list = (IList)Activator.CreateInstance(listType)!;

            if (source is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    var result = this.ExecuteMapping(item, elementTypePair, context);
                    if (result.Exception == null)
                    {
                        list.Add(result.Result);
                    }
                }
            }

            return list;
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

        public void RegisterMapping<TSource, TTarget>(Func<TSource, TTarget> mapping)
        {
            this.RegisterMappingInternal(new DelegateMapping<TSource, TTarget>(mapping));
        }

        private sealed class MappingExceptionCollector
        {
            private readonly List<Exception> exceptions = new();

            public void Add(Exception ex) => this.exceptions.Add(ex);

            public void ThrowIfAny()
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
        }
    }
}