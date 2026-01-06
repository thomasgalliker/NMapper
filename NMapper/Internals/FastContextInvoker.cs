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

        public MappingResult Invoke(object? source, MappingContext context)
        {
            try
            {
                var result = this.map((TSource?)source, context);
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