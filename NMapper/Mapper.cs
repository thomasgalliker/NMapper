using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace NMapper
{
    public sealed class Mapper : IMapper, IMappingContext
    {
        private readonly Dictionary<(Type Source, Type Target), Func<object, object>> map = new Dictionary<(Type, Type), Func<object, object>>();
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
        }

        public void RegisterMapping(IMapping mapping)
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

                var withContext = mappingInterface.GetGenericTypeDefinition() == typeof(IMappingWithContext<,>);
                if (!withContext)
                {
                    var mapMethodInfo = mappingInterface.GetMethod(nameof(IMapping<object, object>.Map))!;
                    this.map[(sourceType, targetType)] = source => this.Map(sourceType, targetType, mapping, source, mapMethodInfo)!;
                }
                else
                {
                    var mapMethodInfo = mappingInterface.GetMethod(nameof(IMappingWithContext<object, object>.Map))!;
                    this.map[(sourceType, targetType)] = source => this.MapWithContext(sourceType, targetType, mapping, source, mapMethodInfo)!;
                }
            }
        }

        private object Map(Type sourceType, Type targetType, IMapping mapping, object source, MethodInfo mapMethodInfo)
        {
            var mappingType = mapping.GetType();

            try
            {
                this.logger.LogDebug($"Map from {sourceType.Name} → {targetType.Name} using {mappingType.Name}");
                return mapMethodInfo.Invoke(mapping, new[] { source });
            }
            catch (TargetInvocationException ex)
            {
                var innerException = ex.InnerException != null ? ex.InnerException : ex;
                throw new MappingException(sourceType, targetType, mapping.GetType(), innerException);
            }
            catch (Exception ex)
            {
                throw new MappingException(sourceType, targetType, mapping.GetType(), ex);
            }
        }

        private object MapWithContext(Type sourceType, Type targetType, IMapping mapping, object source, MethodInfo mapMethodInfo)
        {
            var mappingType = mapping.GetType();

            try
            {
                this.logger.LogDebug($"Map from {sourceType.Name} → {targetType.Name} using {mappingType.Name}");
                return mapMethodInfo.Invoke(mapping, new[] { source, this });
            }
            catch (TargetInvocationException ex)
            {
                var innerException = ex.InnerException != null ? ex.InnerException : ex;
                throw new MappingException(sourceType, targetType, mapping.GetType(), innerException);
            }
            catch (Exception ex)
            {
                throw new MappingException(sourceType, targetType, mappingType, ex);
            }
        }

        public IEnumerable<(Type, Type)> Mappings => this.map.Keys;

        private TTarget? Map<TSource, TTarget>(TSource? source)
        {
            return this.Map<TTarget>(source);
        }

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TTarget>(object? source)
        {
            if (source == null)
            {
                return default;
            }

            var targetType = typeof(TTarget);
            var sourceType = source.GetType();

            if (TryGetEnumerableElementType(sourceType, out var sourceElementType))
            {
                // Array target
                if (targetType.IsArray)
                {
                    var targetElementType = targetType.GetElementType()!;
                    return (TTarget)this.MapArray(source, sourceElementType, targetElementType);
                }

                // IEnumerable<T> target
                {
                    if (TryGetEnumerableElementType(targetType, out var targetElementType))
                    {
                        return (TTarget)this.MapEnumerable((IEnumerable)source, sourceElementType, targetElementType);
                    }
                }
            }

            if (!this.map.TryGetValue((sourceType, targetType), out var map))
            {
                throw new MissingMappingException(sourceType, targetType);
            }

            return (TTarget)map(source);
        }

        private object MapArray(object source, Type sourceElementType, Type targetElementType)
        {
            var list = new List<object?>();

            foreach (var item in (IEnumerable)source)
            {
                list.Add(this.MapInternal(item, sourceElementType, targetElementType));
            }

            var array = Array.CreateInstance(targetElementType, list.Count);
            for (var i = 0; i < list.Count; i++)
            {
                array.SetValue(list[i], i);
            }

            return array;
        }

        private object MapEnumerable(IEnumerable source, Type sourceElementType, Type targetElementType)
        {
            var listType = typeof(List<>).MakeGenericType(targetElementType);
            var list = (IList)Activator.CreateInstance(listType)!;

            foreach (var item in source)
            {
                var mappedItem = this.MapInternal(item, sourceElementType, targetElementType);
                list.Add(mappedItem);
            }

            return list;
        }

        private object? MapInternal(object? source, Type sourceType, Type targetType)
        {
            if (source == null)
            {
                return null;
            }

            if (!this.map.TryGetValue((sourceType, targetType), out var map))
            {
                throw new MissingMappingException(sourceType, targetType);
            }

            return map(source);
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