namespace ObjectMapper
{
    public class DuplicateMappingException : Exception
    {
        public DuplicateMappingException(Type sourceType, Type targetType)
            : base($"Duplicate mapping registered for {sourceType.Name} → {targetType.Name}")
        {
            this.SourceType = sourceType;
            this.TargetType = targetType;
        }

        public Type SourceType { get; }

        public Type TargetType { get; }
    }
}