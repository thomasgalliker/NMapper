namespace NMapper.Internals
{
    internal abstract class FastInvoker
    {
        public static IFastInvoker Create(TypePair typePair, IMapping mapping, Type mappingInterface)
        {
            var method = mappingInterface.GetMethod("Map")!;
            var delType = typeof(Func<,>).MakeGenericType(typePair.SourceType, typePair.TargetType);
            var del = Delegate.CreateDelegate(delType, mapping, method);
            var invokerType = typeof(FastInvoker<,>).MakeGenericType(typePair.SourceType, typePair.TargetType);
            var fastInvoker = (IFastInvoker)Activator.CreateInstance(invokerType, del, typePair, mapping.GetType())!;
            return fastInvoker;
        }
    }

    internal sealed class FastInvoker<TSource, TTarget> : IFastInvoker
    {
        private readonly Func<TSource?, TTarget?> map;
        private readonly TypePair typePair;
        private readonly Type mappingType;

        public FastInvoker(Func<TSource?, TTarget?> map, TypePair typePair, Type mappingType)
        {
            this.map = map;
            this.typePair = typePair;
            this.mappingType = mappingType;
        }

        public bool TryCreateCollectionMappingPlan(Type targetCollectionType, out IFastCollectionMappingPlan? plan)
        {
            if (targetCollectionType.IsArray)
            {
                plan = new FastArrayCollectionMappingPlan<TSource, TTarget>(this.map, this.typePair, this.mappingType);
                return true;
            }

            try
            {
                var adapter = CollectionAdapterFactory.Create(targetCollectionType, typeof(TTarget));
                plan = new FastEnumerableCollectionMappingPlan<TSource, TTarget>(this.map, this.typePair, this.mappingType, adapter);
                return true;
            }
            catch (NotSupportedException)
            {
            }

            plan = null;
            return false;
        }

        public MappingResult Invoke(object? source, MappingContext context)
        {
            try
            {
                var result = this.map((TSource?)source);
                return new MappingResult(result, null, context);
            }
            catch (Exception ex)
            {
                var mappingException = new MappingException(this.typePair.SourceType, this.typePair.TargetType, this.mappingType, ex);
                context.AddException(mappingException);
                return new MappingResult(null, mappingException, context);
            }
        }
    }
}
