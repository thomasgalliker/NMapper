using NMapper.Extensions;

namespace NMapper
{
    public class MissingMappingException : Exception, IEquatable<MissingMappingException>
    {
        public MissingMappingException(Type sourceType, Type targetType)
            : base(GetMessage(sourceType, targetType))
        {
            this.SourceType = sourceType;
            this.TargetType = targetType;
        }

        private static string GetMessage(Type sourceType, Type targetType)
        {
            var sourceTypeName = sourceType.GetFormattedName();
            var targetTypeName = targetType.GetFormattedName();

            if (sourceTypeName == targetTypeName)
            {
                sourceTypeName = sourceType.GetFormattedFullname();
                targetTypeName = targetType.GetFormattedFullname();
            }

            return $"No mapping registered for {sourceTypeName} to {targetTypeName}";
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public bool Equals(MissingMappingException? other)
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
                && this.TargetType == other.TargetType;
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as MissingMappingException);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + this.SourceType.GetHashCode();
                hash = (hash * 23) + this.TargetType.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(MissingMappingException? left, MissingMappingException? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MissingMappingException? left, MissingMappingException? right)
        {
            return !Equals(left, right);
        }
    }
}
