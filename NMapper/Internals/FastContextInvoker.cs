namespace NMapper.Internals
{
    internal abstract class FastContextInvoker
    {
        public static IFastInvoker Create(TypePair typePair, IMapping mapping, Type mappingInterface)
        {
            var method = mappingInterface.GetMethod("Map")!;
            var delType = typeof(Func<,,>).MakeGenericType(typePair.SourceType, typeof(MappingContext), typePair.TargetType);
            var del = Delegate.CreateDelegate(delType, mapping, method);
            var invokerType = typeof(FastContextInvoker<,>).MakeGenericType(typePair.SourceType, typePair.TargetType);
            var fastInvoker = (IFastInvoker)Activator.CreateInstance(invokerType, del, typePair, mapping.GetType())!;
            return fastInvoker;
        }
    }

    internal sealed class FastContextInvoker<TSource, TTarget> : IFastInvoker
    {
        private readonly Func<TSource?, MappingContext, TTarget?> map;
        private readonly TypePair typePair;
        private readonly Type mappingType;

        public FastContextInvoker(Func<TSource?, MappingContext, TTarget?> map, TypePair typePair, Type mappingType)
        {
            this.map = map;
            this.typePair = typePair;
            this.mappingType = mappingType;
        }

        public bool TryCreateCollectionMappingPlan(Type targetCollectionType, [NotNullWhen(true)] out IFastCollectionMappingPlan? plan)
        {
            if (targetCollectionType.IsArray)
            {
                plan = new FastContextArrayCollectionMappingPlan<TSource, TTarget>(this.map, this.typePair, this.mappingType);
                return true;
            }

            try
            {
                var adapter = CollectionAdapterFactory.Create(targetCollectionType, typeof(TTarget));
                plan = new FastContextEnumerableCollectionMappingPlan<TSource, TTarget>(this.map, this.typePair, this.mappingType, adapter);
                return true;
            }
            catch (NotSupportedException)
            {
            }

            plan = null;
            return false;
        }

        public object? Invoke(object? source, MappingContext context)
        {
            try
            {
                return this.map((TSource?)source, context);
            }
            catch (Exception ex)
            {
                if (ex is MappingException or MissingMappingException)
                {
                    throw;
                }

                throw new MappingException(this.typePair.SourceType, this.typePair.TargetType, this.mappingType, ex);
            }
        }
    }
}
