using System.Collections;
using System.Linq.Expressions;

namespace NMapper.Internals
{
    internal class FastCollectionFactory : ICollectionFactory
    {
        private readonly Dictionary<Type, Func<int, Array>> arrayFactories = new();
        private readonly Dictionary<Type, Func<IList>> listFactories = new();

        public Array CreateArray(Type elementType, int length)
        {
            if (!this.arrayFactories.TryGetValue(elementType, out var factory))
            {
                factory = this.CreateArrayFactory(elementType);
                this.arrayFactories[elementType] = factory;
            }

            return factory(length);
        }

        public IList CreateList(Type elementType)
        {
            if (!this.listFactories.TryGetValue(elementType, out var factory))
            {
                factory = this.CreateListFactory(elementType);
                this.listFactories[elementType] = factory;
            }

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
