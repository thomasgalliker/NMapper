namespace NMapper.Internals
{
    internal readonly struct CollectionMappingInfo
    {
        public CollectionMappingInfo(TypePair typePair, Func<Type, EnumerableTypeInfo> getEnumerableTypeInfo)
            : this()
        {
            var sourceEnumerableTypeInfo = getEnumerableTypeInfo(typePair.SourceType);
            if (!sourceEnumerableTypeInfo.IsEnumerable)
            {
                return;
            }

            if (typePair.TargetType.IsArray)
            {
                this.Kind = CollectionMappingKind.Array;
                this.ElementTypePair = new TypePair(sourceEnumerableTypeInfo.ElementType!, typePair.TargetType.GetElementType()!);
                return;
            }

            var targetEnumerableTypeInfo = getEnumerableTypeInfo(typePair.TargetType);
            if (!targetEnumerableTypeInfo.IsEnumerable)
            {
                return;
            }

            this.Kind = CollectionMappingKind.Enumerable;
            this.ElementTypePair = new TypePair(sourceEnumerableTypeInfo.ElementType!, targetEnumerableTypeInfo.ElementType!);
        }

        public CollectionMappingKind Kind { get; }

        public TypePair ElementTypePair { get; }
    }
}
