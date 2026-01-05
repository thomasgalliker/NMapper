using System.Diagnostics.CodeAnalysis;

namespace NMapper.Internals
{
    internal sealed class MappingContext : IMappingContext
    {
        private readonly Mapper mapper;
        private readonly List<Exception> exceptions = new();

        internal MappingContext(Mapper mapper)
        {
            this.mapper = mapper;
        }

        public void AddException(Exception exception)
        {
            if (!this.exceptions.Contains(exception))
            {
                this.exceptions.Add(exception);
            }
        }

        public TTarget? Map<TTarget>(object? source)
        {
            var sourceType = source?.GetType() ?? null;
            return this.Map<TTarget>(source, sourceType);
        }

        [return: NotNullIfNotNull(nameof(source))]
        public TTarget? Map<TSource, TTarget>(TSource? source)
        {
            var sourceType = typeof(TSource);
            return this.Map<TTarget>(source, sourceType);
        }

        private TTarget? Map<TTarget>(object? source, Type? sourceType)
        {
            var result = this.mapper.MapInternal<TTarget>(source, sourceType, this);
            if (result.Exception != null)
            {
                this.AddException(result.Exception);
                return default;
            }
            else
            {
                return (TTarget?)result.Result;
            }
        }

        public void ThrowIfAny()
        {
            if (this.exceptions.Count == 1)
            {
                throw this.exceptions[0];
            }

            if (this.exceptions.Count > 1)
            {
                throw new AggregateException(this.exceptions);
            }
        }
    }

}