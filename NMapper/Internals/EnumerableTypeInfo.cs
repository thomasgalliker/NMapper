namespace NMapper.Internals
{
    internal readonly struct EnumerableTypeInfo
    {
        internal EnumerableTypeInfo(Type type)
            : this()
        {
            // Ignore strings (IEnumerable<char>)
            if (type == typeof(string))
            {
                this.IsEnumerable = false;
                this.ElementType = null;
                return;
            }

            // Array
            if (type.IsArray)
            {
                this.IsEnumerable = true;
                this.ElementType = type.GetElementType()!;
                return;
            }

            // IEnumerable<T> itself
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                this.IsEnumerable = true;
                this.ElementType = type.GetGenericArguments()[0];
                return;
            }

            // Implementations of IEnumerable<T>
            var enumerableInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
            {
                this.IsEnumerable = true;
                this.ElementType = enumerableInterface.GetGenericArguments()[0];
                return;
            }

            this.IsEnumerable = false;
            this.ElementType = null;
        }

        public Type? ElementType { get; }

        public bool IsEnumerable { get; }
    }
}
