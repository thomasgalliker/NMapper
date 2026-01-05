using System.Diagnostics;

namespace NMapper
{
    [DebuggerDisplay("DelegateMapping: {NMapper.Extensions.TypeExtensions.GetFormattedName(typeof(TSource))} to {NMapper.Extensions.TypeExtensions.GetFormattedName(typeof(TTarget))}")]
    public class DelegateMapping<TSource, TTarget> : IMapping<TSource, TTarget>
    {
        private readonly Func<TSource, TTarget> map;

        public DelegateMapping(Func<TSource, TTarget> map)
        {
            this.map = map;
        }

        public TTarget Map(TSource source)
        {
            return this.map(source);
        }
    }
}