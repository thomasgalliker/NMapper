using NMapper.Extensions;

namespace NMapper
{
    public class MappingException : Exception, IEquatable<MappingException>
    {
        public MappingException(Type sourceType, Type targetType, Type mappingType, Exception ex)
            : base(GetMessage(sourceType, targetType, mappingType), ex)
        {
            this.SourceType = sourceType;
            this.TargetType = targetType;
            this.MappingType = mappingType;
        }

        private static string GetMessage(Type sourceType, Type targetType, Type mappingType)
        {
            var sourceTypeName = sourceType.GetFormattedName();
            var targetTypeName = targetType.GetFormattedName();
            var mappingTypeName = mappingType.GetFormattedName();

            if (sourceTypeName == targetTypeName)
            {
                sourceTypeName = sourceType.GetFormattedFullname();
                targetTypeName = targetType.GetFormattedFullname();
            }

            return $"{mappingTypeName} failed to map from {sourceTypeName} to {targetTypeName}";
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public Type MappingType { get; }

        public bool Equals(MappingException? other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.SourceType == other.SourceType
                && this.TargetType == other.TargetType
                && this.MappingType == other.MappingType;
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as MappingException);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + this.SourceType.GetHashCode();
                hash = (hash * 23) + this.TargetType.GetHashCode();
                hash = (hash * 23) + this.MappingType.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(MappingException? left, MappingException? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MappingException? left, MappingException? right)
        {
            return !Equals(left, right);
        }
    }
}
