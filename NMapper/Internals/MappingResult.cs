namespace NMapper.Internals
{
    internal readonly struct MappingResult
    {
        public MappingResult(object? result, Exception? exception, MappingContext context)
            : this()
        {
            this.Result = result;
            this.Exception = exception;
            this.Success = exception == null;
            this.Context = context;
        }

        public object? Result { get; }

        public Exception? Exception { get; }

        public bool Success { get; }

        public MappingContext Context { get; }
    }
}