using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace NMapper.Internals
{
    internal class FastCollectionFactory : ICollectionFactory
    {
        private readonly ConcurrentDictionary<Type, Func<int, Array>> arrayFactories = new();
        private readonly ConcurrentDictionary<(Type CollectionType, Type ElementType), Func<int?, ObjectCollection>> collectionFactories = new();

        public Array CreateArray(Type elementType, int length)
        {
            var factory = this.arrayFactories.GetOrAdd(elementType, CreateArrayFactory);
            return factory(length);
        }

        public ObjectCollection CreateCollection(Type collectionType, Type elementType, int? capacity = null)
        {
            var factory = this.collectionFactories.GetOrAdd((collectionType, elementType), key => this.CreateCollectionFactory(key.CollectionType, key.ElementType));
            return factory(capacity);
        }

        private static Func<int, Array> CreateArrayFactory(Type elementType)
        {
            var lengthParam = Expression.Parameter(typeof(int), "length");
            var newArray = Expression.NewArrayBounds(elementType, lengthParam);
            var cast = Expression.Convert(newArray, typeof(Array));

            return Expression.Lambda<Func<int, Array>>(cast, lengthParam).Compile();
        }

        private Func<int?, ObjectCollection> CreateCollectionFactory(Type collectionType, Type elementType)
        {
            var concreteCollectionType = CollectionAdapterFactory.ResolveCollectionType(collectionType, elementType);
            var collectionFactory = CreateObjectFactory(concreteCollectionType);
            var addMethod = CreateAddMethod(concreteCollectionType, elementType);

            return capacity => new ObjectCollection(collectionFactory(capacity), addMethod);
        }

        private static Func<int?, object> CreateObjectFactory(Type collectionType)
        {
            var intCtor = collectionType.GetConstructor(new[] { typeof(int) });
            var defaultCtor = collectionType.GetConstructor(Type.EmptyTypes);

            if (intCtor == null && defaultCtor == null)
            {
                throw new NotSupportedException(
                    $"Collection type '{collectionType}' must have a parameterless constructor or a constructor with a single Int32 capacity parameter.");
            }

            if (intCtor != null && defaultCtor != null)
            {
                return capacity => capacity.HasValue ? intCtor.Invoke(new object[] { capacity.Value }) : defaultCtor.Invoke(null)!;
            }

            if (intCtor != null)
            {
                return capacity => intCtor.Invoke(new object[] { capacity ?? 0 })!;
            }

            return _ => defaultCtor!.Invoke(null)!;
        }

        private static Action<object, object?> CreateAddMethod(Type collectionType, Type elementType)
        {
            var addMethod = collectionType.GetMethod("Add", new[] { elementType });
            if (addMethod == null)
            {
                throw new NotSupportedException($"Collection type '{collectionType}' must expose an Add({elementType.Name}) method.");
            }

            var collectionParameter = Expression.Parameter(typeof(object), "collection");
            var itemParameter = Expression.Parameter(typeof(object), "item");

            var body = Expression.Call(
                Expression.Convert(collectionParameter, collectionType),
                addMethod,
                Expression.Convert(itemParameter, elementType));

            return Expression.Lambda<Action<object, object?>>(body, collectionParameter, itemParameter).Compile();
        }
    }
}
