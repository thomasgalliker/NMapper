using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NMapper.Extensions;

namespace NMapper
{
    public sealed class Mapper : IMapper
    {
        private readonly Dictionary<(Type Source, Type Target), Func<(object? Source, MappingContext Context), (object? Result, Exception? Exception, MappingContext Context)>> map = new Dictionary<(Type, Type), Func<(object?, MappingContext), (object?, Exception?, MappingContext)>>();
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
                var sourceType = args[0];
                var targetType = args[1];

                if (this.map.ContainsKey((sourceType, targetType)))
                {
                    throw new DuplicateMappingException(sourceType, targetType);
                }

                var useContext = mappingInterface.GetGenericTypeDefinition() == typeof(IMappingWithContext<,>);
                if (!useContext)
                {
                    var methodInfoMap = mappingInterface.GetMethod(nameof(IMappingWithContext<object, object>.Map))!;
                    this.map[(sourceType, targetType)] = (p) => this.Map(sourceType, targetType, mapping, p.Source, p.Context, useContext, methodInfoMap)!;
                }
                else
                {
                    var methodInfoMapWithContext = mappingInterface.GetMethod(nameof(IMapping<object, object>.Map))!;
                    this.map[(sourceType, targetType)] = (p) => this.Map(sourceType, targetType, mapping, p.Source, p.Context, useContext, methodInfoMapWithContext)!;
                }
            }
        }

        private (object?, Exception?, MappingContext) Map(Type sourceType, Type targetType, IMapping mapping, object? source, MappingContext context, bool withContext, MethodInfo methodInfo)
        {
            var mappingType = mapping.GetType();

            try
            {
                this.logger.LogDebug($"Map from {sourceType.GetFormattedName()} → {targetType.GetFormattedName()} using {mappingType.GetFormattedName()}");

                object? result;

                if (withContext)
                {
                    result = methodInfo.Invoke(mapping, new[] { source, context });
                }
                else
                {
                    result = methodInfo.Invoke(mapping, new[] { source });
                }

                return (result, null, context);
            }
            catch (TargetInvocationException ex)
            {
                var innerException = ex.InnerException != null ? ex.InnerException : ex;
                var mappingException = new MappingException(sourceType, targetType, mapping.GetType(), innerException);
                context.AddException(mappingException);
                return (default, mappingException, context);
            }
            catch (Exception ex)
            {
                var mappingException = new MappingException(sourceType, targetType, mapping.GetType(), ex);
                context.AddException(mappingException);
                return (default, mappingException, context);
            }
        }

        public IEnumerable<(Type, Type)> Mappings => this.map.Keys;

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TTarget>(object? source)
        {
            var sourceType = source?.GetType() ?? null;
            var context = new MappingContext(this);
            var result = this.MapInternal<TTarget>(source, sourceType, context);

            context.ThrowIfAny();

            return result.Result;
        }

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TSource, TTarget>(TSource? source)
        {
            var sourceType = typeof(TSource);

            var context = new MappingContext(this);
            var result = this.MapInternal<TTarget>(source, sourceType, context);

            context.ThrowIfAny();

            return result.Result;
        }

        internal (TTarget? Result, Exception? Exception) MapInternal<TTarget>(object? source, Type? sourceType, MappingContext context)
        {
            if (sourceType == null)
            {
                return default;
            }

            var targetType = typeof(TTarget);

            if (TryGetEnumerableElementType(sourceType, out var sourceElementType))
            {
                // Array T[]
                if (targetType.IsArray)
                {
                    var targetElementType = targetType.GetElementType()!;
                    var array = this.MapArray(source, sourceElementType, targetElementType, context);
                    return ((TTarget?)array, null);
                }

                // IEnumerable<T>
                {
                    if (TryGetEnumerableElementType(targetType, out var targetElementType))
                    {
                        var list = this.MapEnumerable(source, sourceElementType, targetElementType, context);
                        return ((TTarget?)list, null);
                    }
                }
            }

            var result = this.ExecuteMapping(source, sourceType, targetType, context);
            if (result.Exception == null)
            {
                return ((TTarget?)result.Result, null);
            }

            return (default, result.Exception);
        }

        private (object? Result, Exception? Exception) ExecuteMapping(object? source, Type sourceType, Type targetType, MappingContext context)
        {
            if (sourceType == null)
            {
                return (null, null);
            }

            if (!this.map.TryGetValue((sourceType, targetType), out var map))
            {
                var ex = new MissingMappingException(sourceType, targetType);
                context.AddException(ex);
                return (null, ex);
            }

            try
            {
                var result = map((source, context));
                return (result.Result, result.Exception);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }

        private object MapArray(object? source, Type sourceElementType, Type targetElementType, MappingContext context)
        {
            var list = new List<object?>();

            if (source != null)
            {
                foreach (var item in (IEnumerable)source)
                {
                    (var mapped, var ex) = this.ExecuteMapping(item, sourceElementType, targetElementType, context);
                    if (ex == null)
                    {
                        list.Add(mapped);
                    }
                }
            }

            var array = Array.CreateInstance(targetElementType, list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                array.SetValue(list[i], i);
            }

            return array;
        }

        private object MapEnumerable(object? source, Type sourceElementType, Type targetElementType, MappingContext context)
        {
            var listType = typeof(List<>).MakeGenericType(targetElementType);
            var list = (IList)Activator.CreateInstance(listType)!;

            if (source != null)
            {
                foreach (var item in (IEnumerable)source)
                {
                    (var mapped, var ex) = this.ExecuteMapping(item, sourceElementType, targetElementType, context);
                    if (ex == null)
                    {
                        list.Add(mapped);
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