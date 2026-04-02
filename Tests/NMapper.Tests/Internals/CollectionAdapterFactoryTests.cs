using System.Collections.ObjectModel;
using FluentAssertions;
using NMapper.Internals;
using Xunit;

namespace NMapper.Tests.Internals
{
    public sealed class CollectionAdapterFactoryTests
    {
        [Fact]
        public void Create_CreatesListAdapter()
        {
            var adapter = CollectionAdapterFactory.Create(typeof(List<int>), typeof(int));

            var collection = adapter.Create(capacity: 2);
            adapter.Add(collection, 1);
            adapter.Add(collection, 2);

            collection.Should().BeOfType<List<int>>();
            ((List<int>)collection).Should().ContainInOrder(1, 2);
        }

        [Fact]
        public void Create_CreatesHashSetAdapter_ForISetInterface()
        {
            var adapter = CollectionAdapterFactory.Create(typeof(ISet<int>), typeof(int));

            var collection = adapter.Create(capacity: null);
            adapter.Add(collection, 1);
            adapter.Add(collection, 2);

            collection.Should().BeOfType<HashSet<int>>();
            ((HashSet<int>)collection).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public void Create_CreatesConcreteCollectionAdapter()
        {
            var adapter = CollectionAdapterFactory.Create(typeof(Collection<string>), typeof(string));

            var collection = adapter.Create(capacity: null);
            adapter.Add(collection, "a");
            adapter.Add(collection, "b");

            collection.Should().BeOfType<Collection<string>>();
            ((Collection<string>)collection).Should().ContainInOrder("a", "b");
        }

        [Fact]
        public void ResolveCollectionType_MapsReadOnlyInterfacesToList()
        {
            var resolved = CollectionAdapterFactory.ResolveCollectionType(typeof(IReadOnlyList<int>), typeof(int));

            resolved.Should().Be(typeof(List<int>));
        }

        [Fact]
        public void ResolveCollectionType_ThrowsForArray()
        {
            Action action = () => CollectionAdapterFactory.ResolveCollectionType(typeof(int[]), typeof(int));

            action.Should().Throw<NotSupportedException>();
        }
    }
}
