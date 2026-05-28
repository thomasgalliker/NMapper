namespace NMapper.Tests
{
    public class TypePairTests
    {
        [Fact]
        public void TypePair_ShouldInitializeWithBothTypes()
        {
            // Arrange
            var sourceType = typeof(string);
            var targetType = typeof(int);

            // Act
            var typePair = new TypePair(sourceType, targetType);

            // Assert
            typePair.SourceType.Should().Be(sourceType);
            typePair.TargetType.Should().Be(targetType);
        }

        [Fact]
        public void TypePair_ShouldHandleNullSourceType()
        {
            // Act
            var typePair = new TypePair(null!, typeof(int));

            // Assert
            typePair.SourceType.Should().BeNull();
            typePair.TargetType.Should().Be(typeof(int));
        }

        [Fact]
        public void TypePair_ShouldHandleNullTargetType()
        {
            // Act
            var typePair = new TypePair(typeof(string), null!);

            // Assert
            typePair.SourceType.Should().Be(typeof(string));
            typePair.TargetType.Should().BeNull();
        }

        [Fact]
        public void TypePair_ShouldHandleBothNullTypes()
        {
            // Act
            var typePair = new TypePair(null!, null!);

            // Assert
            typePair.SourceType.Should().BeNull();
            typePair.TargetType.Should().BeNull();
        }

        [Fact]
        public void TypePair_ShouldBeEqualWhenTypesMatch()
        {
            // Arrange
            var typePair1 = new TypePair(typeof(string), typeof(int));
            var typePair2 = new TypePair(typeof(string), typeof(int));

            // Act & Assert
            typePair1.Should().Be(typePair2);
            typePair1.Equals(typePair2).Should().BeTrue();
            (typePair1 == typePair2).Should().BeTrue();
            (typePair1 != typePair2).Should().BeFalse();
        }

        [Fact]
        public void TypePair_ShouldNotBeEqualWhenSourceTypesDiffer()
        {
            // Arrange
            var typePair1 = new TypePair(typeof(string), typeof(int));
            var typePair2 = new TypePair(typeof(double), typeof(int));

            // Act & Assert
            typePair1.Should().NotBe(typePair2);
            typePair1.Equals(typePair2).Should().BeFalse();
            (typePair1 == typePair2).Should().BeFalse();
            (typePair1 != typePair2).Should().BeTrue();
        }

        [Fact]
        public void TypePair_ShouldNotBeEqualWhenTargetTypesDiffer()
        {
            // Arrange
            var typePair1 = new TypePair(typeof(string), typeof(int));
            var typePair2 = new TypePair(typeof(string), typeof(double));

            // Act & Assert
            typePair1.Should().NotBe(typePair2);
            typePair1.Equals(typePair2).Should().BeFalse();
            (typePair1 == typePair2).Should().BeFalse();
            (typePair1 != typePair2).Should().BeTrue();
        }

        [Fact]
        public void TypePair_ShouldNotBeEqualToNull()
        {
            // Arrange
            var typePair = new TypePair(typeof(string), typeof(int));

            // Act & Assert
            typePair.Should().NotBe(null);
            typePair.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void TypePair_ShouldNotBeEqualToDifferentType()
        {
            // Arrange
            var typePair = new TypePair(typeof(string), typeof(int));
            var differentObject = "not a type pair";

            // Act & Assert
            typePair.Equals(differentObject).Should().BeFalse();
        }

        [Fact]
        public void TypePair_ShouldHaveConsistentHashCode()
        {
            // Arrange
            var typePair1 = new TypePair(typeof(string), typeof(int));
            var typePair2 = new TypePair(typeof(string), typeof(int));

            // Act & Assert
            typePair1.GetHashCode().Should().Be(typePair2.GetHashCode());
        }

        [Fact]
        public void TypePair_ShouldHaveDifferentHashCodeForDifferentTypes()
        {
            // Arrange
            var typePair1 = new TypePair(typeof(string), typeof(int));
            var typePair2 = new TypePair(typeof(double), typeof(int));

            // Act & Assert
            typePair1.GetHashCode().Should().NotBe(typePair2.GetHashCode());
        }

        [Fact]
        public void TypePair_ShouldHandleNullTypesInHashCode()
        {
            // Act
            var typePair = new TypePair(null!, null!);
            var hashCode = typePair.GetHashCode();

            // Assert
            hashCode.Should().Be(0); // Expected behavior for null types
        }

        [Fact]
        public void TypePair_ShouldProvideStringRepresentation()
        {
            // Arrange
            var typePair = new TypePair(typeof(string), typeof(int));

            // Act
            var stringRepresentation = typePair.ToString();

            // Assert
            stringRepresentation.Should().Contain("String");
            stringRepresentation.Should().Contain("Int32");
        }

        [Fact]
        public void TypePair_ShouldHandleGenericTypes()
        {
            // Arrange
            var sourceType = typeof(List<string>);
            var TargetType = typeof(IEnumerable<int>);

            // Act
            var typePair = new TypePair(sourceType, TargetType);

            // Assert
            typePair.SourceType.Should().Be(sourceType);
            typePair.TargetType.Should().Be(TargetType);
        }

        [Fact]
        public void TypePair_ShouldWorkAsHashSetKey()
        {
            // Arrange
            var typePair1 = new TypePair(typeof(string), typeof(int));
            var typePair2 = new TypePair(typeof(string), typeof(int));
            var typePair3 = new TypePair(typeof(double), typeof(int));

#pragma warning disable IDE0028 // Simplify collection initialization
            var hashSet = new HashSet<TypePair>();
#pragma warning restore IDE0028 // Simplify collection initialization

            // Act
            hashSet.Add(typePair1);
            hashSet.Add(typePair2); // Should not be added as it's equal to typePair1
            hashSet.Add(typePair3);

            // Assert
            hashSet.Should().HaveCount(2);
            hashSet.Should().Contain(typePair1);
            hashSet.Should().Contain(typePair3);
        }
    }
}