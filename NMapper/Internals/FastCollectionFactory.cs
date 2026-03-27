using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace NMapper.Internals
{
    internal class FastCollectionFactory : ICollectionFactory
    {
        private readonly ConcurrentDictionary<Type, Func<int, Array>> arrayFactories = new();
        private readonly ConcurrentDictionary<Type, Func<IList>> listFactories = new();

        public Array CreateArray(Type elementType, int length)
        {
            var factory = this.arrayFactories.GetOrAdd(elementType, this.CreateArrayFactory);
            return factory(length);
        }

        public IList CreateList(Type elementType)
        {
            var factory = this.listFactories.GetOrAdd(elementType, this.CreateListFactory);
            return factory();
        }

        private Func<int, Array> CreateArrayFactory(Type elementType)
        {
            var lengthParam = Expression.Parameter(typeof(int), "length");
            var newArray = Expression.NewArrayBounds(elementType, lengthParam);
            var cast = Expression.Convert(newArray, typeof(Array));

            return Expression.Lambda<Func<int, Array>>(cast, lengthParam).Compile();
        }

        private Func<IList> CreateListFactory(Type elementType)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var ctor = listType.GetConstructor(Type.EmptyTypes)!;
            var newExpr = Expression.New(ctor);
            var cast = Expression.Convert(newExpr, typeof(IList));

            return Expression.Lambda<Func<IList>>(cast).Compile();
        }
    }
}
