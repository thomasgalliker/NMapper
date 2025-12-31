namespace ObjectMapper
{
    public class MappingException : Exception
    {
        public MappingException(Type sourceType, Type targetType, Type mappingType, Exception ex)
              : base($"Failed to map from {sourceType.Name} → {targetType.Name} using mapping {mappingType.Name}", ex)
        {
            this.SourceType = sourceType;
            this.TargetType = targetType;
            this.MappingType = mappingType;
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public Type MappingType { get; }
    }
}