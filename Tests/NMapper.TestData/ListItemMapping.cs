using System.Collections.Generic;
using System.Linq;

namespace NMapper.TestData
{
    public class ListItemMapping : IMappingWithContext<List<Item>, List<Item>>
    {
        public List<Item> Map(List<Item> source, IMappingContext context)
        {
            return source.Select(i => context.Map<Item>(i)).ToList();
        }
    }
}