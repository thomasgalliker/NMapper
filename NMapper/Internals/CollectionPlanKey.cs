namespace NMapper.Internals
{
    internal readonly struct CollectionPlanKey : IEquatable<CollectionPlanKey>
    {
        public CollectionPlanKey(Type sourceElementType, Type targetCollectionType)
        {
            this.SourceElementType = sourceElementType;
            this.TargetCollectionType = targetCollectionType;
        }

        public Type SourceElementType { get; }

        public Type TargetCollectionType { get; }

        public bool Equals(CollectionPlanKey other)
        {
            return this.SourceElementType == other.SourceElementType &&
                   this.TargetCollectionType == other.TargetCollectionType;
        }

        public override bool Equals(object? obj)
        {
            return obj is CollectionPlanKey other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.SourceElementType.GetHashCode() * 397) ^ this.TargetCollectionType.GetHashCode();
            }
        }
    }
}
