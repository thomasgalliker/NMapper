using FluentAssertions;
using NMapper.Internals;
using Xunit;

namespace NMapper.Tests.Internals
{
    public class CollectionMappingInfoTests
    {
        [Fact]
        public void Constructor_ShouldReturnNone_WhenSourceTypeIsNotEnumerable()
        {
            // Arrange
            var typePair = new TypePair(typeof(int), typeof(string[]));

            // Act
            var info = new CollectionMappingInfo(typePair, static type => new EnumerableTypeInfo(type));

            // Assert
            info.Kind.Should().Be(CollectionMappingKind.None);
            info.ElementTypePair.SourceType.Should().BeNull();
            info.ElementTypePair.TargetType.Should().BeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateArrayPlan_WhenTargetTypeIsArray()
        {
            // Arrange
            var typePair = new TypePair(typeof(List<int>), typeof(string[]));

            // Act
            var info = new CollectionMappingInfo(typePair, static type => new EnumerableTypeInfo(type));

            // Assert
            info.Kind.Should().Be(CollectionMappingKind.Array);
            info.ElementTypePair.Should().Be(new TypePair(typeof(int), typeof(string)));
        }

        [Fact]
        public void Constructor_ShouldReturnNone_WhenTargetTypeIsNotEnumerableAndNotArray()
        {
            // Arrange
            var typePair = new TypePair(typeof(List<int>), typeof(string));

            // Act
            var info = new CollectionMappingInfo(typePair, static type => new EnumerableTypeInfo(type));

            // Assert
            info.Kind.Should().Be(CollectionMappingKind.None);
            info.ElementTypePair.SourceType.Should().BeNull();
            info.ElementTypePair.TargetType.Should().BeNull();
        }

        [Fact]
        public void Constructor_ShouldCreateEnumerablePlan_WhenBothTypesAreEnumerable()
        {
            // Arrange
            var typePair = new TypePair(typeof(List<int>), typeof(IEnumerable<string>));

            // Act
            var info = new CollectionMappingInfo(typePair, static type => new EnumerableTypeInfo(type));

            // Assert
            info.Kind.Should().Be(CollectionMappingKind.Enumerable);
            info.ElementTypePair.Should().Be(new TypePair(typeof(int), typeof(string)));
        }
    }
}
