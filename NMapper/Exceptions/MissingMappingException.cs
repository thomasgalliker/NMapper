namespace NMapper
{
    public class MissingMappingException : Exception
    {
        public MissingMappingException(Type sourceType, Type targetType)
            : base($"No mapper registered for {sourceType.Name} → {targetType.Name}")
        {
            this.SourceType = sourceType;
            this.TargetType = targetType;
        }

        public Type SourceType { get; }

        public Type TargetType { get; }
    }
}