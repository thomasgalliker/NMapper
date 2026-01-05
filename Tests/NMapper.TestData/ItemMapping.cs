namespace NMapper.TestData
{
    public class ItemMapping : IMapping<Item, Item>
    {
        public Item Map(Item source)
        {
            return new Item
            {
                Id = source.Id,
                Bool = source.Bool,
                Byte = source.Byte,
                Char = source.Char,
                DateTime = source.DateTime,
                Decimal = source.Decimal,
                Float = source.Float,
                Int = source.Int,
                Long = source.Long,
                Short = source.Short,
                FirstName = source.FirstName,
                LastName = source.LastName
            };
        }
    }
}