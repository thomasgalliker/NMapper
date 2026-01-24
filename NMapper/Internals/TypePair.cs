namespace NMapper
{
    public readonly struct TypePair : IEquatable<TypePair>
    {
        public TypePair(Type source, Type target) : this()
        {
            this.TargetType = target;
            this.SourceType = source;
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TypePair pair && this.Equals(pair);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (
                    (this.SourceType != null ? this.SourceType.GetHashCode() : 0) * 397) ^
                    (this.TargetType != null ? this.TargetType.GetHashCode() : 0);
            }
        }

        public bool Equals(TypePair other)
        {
            return
                this.SourceType == other.SourceType &&
                this.TargetType == other.TargetType;
        }

        public static bool operator ==(TypePair left, TypePair right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TypePair left, TypePair right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{this.SourceType.Name} >> {this.TargetType.Name}";
        }
    }
}
