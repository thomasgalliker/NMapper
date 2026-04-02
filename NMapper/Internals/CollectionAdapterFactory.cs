using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace NMapper.Internals
{
    internal static class CollectionAdapterFactory
    {
        private static readonly ConcurrentDictionary<(Type CollectionType, Type ElementType), ICompiledCollectionAdapter> Adapters = new();

        public static ICompiledCollectionAdapter Create(Type collectionType, Type elementType)
        {
            return Adapters.GetOrAdd((collectionType, elementType), key => CreateAdapter(key.CollectionType, key.ElementType));
        }

        private static ICompiledCollectionAdapter CreateAdapter(Type collectionType, Type elementType)
        {
            var concreteCollectionType = ResolveCollectionType(collectionType, elementType);
            var adapterType = typeof(CompiledCollectionAdapter<,>).MakeGenericType(concreteCollectionType, elementType);
            return (ICompiledCollectionAdapter)Activator.CreateInstance(adapterType)!;
        }

        internal static Type ResolveCollectionType(Type collectionType, Type elementType)
        {
            if (collectionType == null)
            {
                throw new ArgumentNullException(nameof(collectionType));
            }

            if (elementType == null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            if (collectionType.IsArray)
            {
                throw new NotSupportedException("Array collections must be created separately.");
            }

            if (collectionType.IsInterface || collectionType.IsAbstract)
            {
                if (!collectionType.IsGenericType)
                {
                    throw new NotSupportedException($"Collection type '{collectionType}' is not supported.");
                }

                var genericTypeDefinition = collectionType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(IEnumerable<>) ||
                    genericTypeDefinition == typeof(ICollection<>) ||
                    genericTypeDefinition == typeof(IList<>) ||
                    genericTypeDefinition == typeof(IReadOnlyCollection<>) ||
                    genericTypeDefinition == typeof(IReadOnlyList<>))
                {
                    return typeof(List<>).MakeGenericType(elementType);
                }

                if (genericTypeDefinition == typeof(ISet<>))
                {
                    return typeof(HashSet<>).MakeGenericType(elementType);
                }

                throw new NotSupportedException($"Collection type '{collectionType}' is not supported.");
            }

            if (!typeof(IEnumerable).IsAssignableFrom(collectionType))
            {
                throw new NotSupportedException($"Collection type '{collectionType}' is not supported.");
            }

            return collectionType;
        }

        private sealed class CompiledCollectionAdapter<TCollection, TItem> : ICompiledCollectionAdapter
        {
            private static readonly Func<int?, object> CreateCollection = CreateCollectionFactory();
            private static readonly Action<object, object?> AddToCollection = CreateAddMethod();

            public object Create(int? capacity)
            {
                return CreateCollection(capacity);
            }

            public void Add(object collection, object? item)
            {
                AddToCollection(collection, item);
            }

            private static Func<int?, object> CreateCollectionFactory()
            {
                var collectionType = typeof(TCollection);
                var intCtor = collectionType.GetConstructor(new[] { typeof(int) });
                var defaultCtor = collectionType.GetConstructor(Type.EmptyTypes);

                if (intCtor == null && defaultCtor == null)
                {
                    throw new NotSupportedException(
                        $"Collection type '{collectionType}' must have a parameterless constructor or a constructor with a single Int32 capacity parameter.");
                }

                if (intCtor != null && defaultCtor != null)
                {
                    return capacity => capacity.HasValue ? intCtor.Invoke(new object[] { capacity.Value })! : defaultCtor.Invoke(null)!;
                }

                if (intCtor != null)
                {
                    return capacity => intCtor.Invoke(new object[] { capacity ?? 0 })!;
                }

                return _ => defaultCtor!.Invoke(null)!;
            }

            private static Action<object, object?> CreateAddMethod()
            {
                var collectionType = typeof(TCollection);
                var itemType = typeof(TItem);
                var addMethod = collectionType.GetMethod("Add", new[] { itemType });
                if (addMethod == null)
                {
                    throw new NotSupportedException($"Collection type '{collectionType}' must expose an Add({itemType.Name}) method.");
                }

                var collectionParameter = Expression.Parameter(typeof(object), "collection");
                var itemParameter = Expression.Parameter(typeof(object), "item");

                var body = Expression.Call(
                    Expression.Convert(collectionParameter, collectionType),
                    addMethod,
                    Expression.Convert(itemParameter, itemType));

                return Expression.Lambda<Action<object, object?>>(body, collectionParameter, itemParameter).Compile();
            }
        }
    }
}
