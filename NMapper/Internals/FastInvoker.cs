namespace NMapper.Internals
{
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
