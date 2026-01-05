using System;
using System.Collections.Generic;

namespace NMapper.TestData
{
    public static class SourceWithCollectionsHelper
    {
        public static SourceWithCollections CreateSource(int collectionLength)
        {
            var source = new SourceWithCollections();

            for (var i = 0; i < collectionLength; i++)
            {
                source.StringList.Add(Guid.NewGuid().ToString());
                source.ItemList.Add(CreateItem(i));
            }

            return source;
        }

        private static Item CreateItem(int id)
        {
            return new Item
            {
                Id = id,
                FirstName = "John",
                LastName = "Doe",
                Short = 3,
                Long = 10,
                Int = 5,
                Float = 4.9f,
                Decimal = 4.0m,
                DateTime = DateTime.Now,
                Char = 'a',
                Bool = true,
                Byte = 0
            };
        }
    }

    public class SourceWithCollections
    {
        public SourceWithCollections()
        {
            this.ItemList = new List<Item>();
            this.StringList = new List<string>();
        }

        public List<Item> ItemList { get; set; }

        public List<string> StringList { get; set; }
    }


    public class TargetWithCollections
    {
        public TargetWithCollections()
        {
            this.ItemList = new List<Item>();
            this.StringList = new List<string>();
        }

        public List<Item> ItemList { get; set; }

        public List<string> StringList { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public bool Bool { get; set; }
        public byte Byte { get; set; }
        public char Char { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Decimal { get; set; }
        public string? FirstName { get; set; }
        public float Float { get; set; }
        public int Int { get; set; }
        public string? LastName { get; set; }
        public long Long { get; set; }
        public short Short { get; set; }
    }
}
