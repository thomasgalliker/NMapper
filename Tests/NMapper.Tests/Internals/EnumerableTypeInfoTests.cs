namespace NMapper.Tests.Internals
{
    public class EnumerableTypeInfoTests
    {
        [Fact]
        public void Constructor_ShouldTreatStringAsNotEnumerable()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var enumerableTypeInfo = new EnumerableTypeInfo(type);

            // Assert
            enumerableTypeInfo.IsEnumerable.Should().BeFalse();
            enumerableTypeInfo.ElementType.Should().BeNull();
        }

        [Fact]
        public void Constructor_ShouldDetectArrayElementType()
        {
            // Arrange
            var type = typeof(int[]);

            // Act
            var enumerableTypeInfo = new EnumerableTypeInfo(type);

            // Assert
            enumerableTypeInfo.IsEnumerable.Should().BeTrue();
            enumerableTypeInfo.ElementType.Should().Be(typeof(int));
        }

        [Fact]
        public void Constructor_ShouldDetectIEnumerableElementType()
        {
            // Arrange
            var type = typeof(IEnumerable<int>);

            // Act
            var enumerableTypeInfo = new EnumerableTypeInfo(type);

            // Assert
            enumerableTypeInfo.IsEnumerable.Should().BeTrue();
            enumerableTypeInfo.ElementType.Should().Be(typeof(int));
        }

        [Fact]
        public void Constructor_ShouldDetectConcreteGenericEnumerableElementType()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var enumerableTypeInfo = new EnumerableTypeInfo(type);

            // Assert
            enumerableTypeInfo.IsEnumerable.Should().BeTrue();
            enumerableTypeInfo.ElementType.Should().Be(typeof(int));
        }

        [Fact]
        public void Constructor_ShouldDetectCustomEnumerableElementType()
        {
            // Arrange
            var type = typeof(TestEnumerable);

            // Act
            var enumerableTypeInfo = new EnumerableTypeInfo(type);

            // Assert
            enumerableTypeInfo.IsEnumerable.Should().BeTrue();
            enumerableTypeInfo.ElementType.Should().Be(typeof(int));
        }

        [Fact]
        public void Constructor_ShouldTreatNonEnumerableTypeAsNotEnumerable()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var enumerableTypeInfo = new EnumerableTypeInfo(type);

            // Assert
            enumerableTypeInfo.IsEnumerable.Should().BeFalse();
            enumerableTypeInfo.ElementType.Should().BeNull();
        }

        private sealed class TestEnumerable : IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator()
            {
                yield break;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
