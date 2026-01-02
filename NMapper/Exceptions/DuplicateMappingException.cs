using NMapper.Extensions;

namespace NMapper
{
    public class DuplicateMappingException : Exception, IEquatable<DuplicateMappingException>
    {
        public DuplicateMappingException(Type sourceType, Type targetType)
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

            return $"Duplicate mapping registered for {sourceTypeName} to {targetTypeName}";
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public bool Equals(DuplicateMappingException? other)
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

        public override bool Equals(object obj)
        {
            return this.Equals(obj as DuplicateMappingException);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (this.SourceType != null ? this.SourceType.GetHashCode() : 0);
                hash = (hash * 23) + (this.TargetType != null ? this.TargetType.GetHashCode() : 0);
                return hash;
            }
        }

        public static bool operator ==(DuplicateMappingException? left, DuplicateMappingException? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DuplicateMappingException? left, DuplicateMappingException? right)
        {
            return !Equals(left, right);
        }
    }
}
