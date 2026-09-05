using System;
using System.Collections.Generic;

namespace NMapper.TestData.Benchmarks
{
    public static class SourceWithCollectionsHelper
    {
        public static SourceWithCollections CreateSource(int collectionLength)
        {
            var source = new SourceWithCollections();

            for (var i = 0; i < collectionLength; i++)
            {
                source.StringList.Add(new string('X', 100));
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
                DateTime = DateTime.MinValue,
                Char = 'a',
                Bool = true,
                Byte = 0
            };
        }
    }

    /// <summary>
    /// Throughput fixture for the performance test and the benchmark. It is deliberately flat and
    /// free of circular references, and is not part of the Person/Family test domain.
    /// </summary>
    /// <remarks>
    /// <see cref="Item"/> carries one property per primitive type on purpose, so that the
    /// benchmark measures per-property copy cost rather than graph traversal. The competing
    /// mappers it is compared against resolve members by convention and do not track references,
    /// so a fixture with back-references could not be mapped by them at all.
    /// </remarks>
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
            this.ItemList = new List<ItemDto>();
            this.StringList = new List<string>();
        }

        public List<ItemDto> ItemList { get; set; }

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

    public class ItemDto
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
