using System.Collections.Generic;

namespace NMapper.TestData
{
    public class SourceToTargetCollectionsMapping : IMappingWithContext<SourceWithCollections, TargetWithCollections>
    {
        public TargetWithCollections Map(SourceWithCollections source, IMappingContext context)
        {
            return new TargetWithCollections
            {
                StringList = source.StringList,
                ItemList = context.Map<List<ItemDto>>(source.ItemList)
            };
        }
    }
}