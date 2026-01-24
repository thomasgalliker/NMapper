using System.Collections;
using FluentAssertions;
using NMapper.Internals;
using Xunit;

namespace NMapper.Tests.Internals
{
    public sealed class FastCollectionFactoryTests
    {
        private readonly FastCollectionFactory fastCollectionFactory = new FastCollectionFactory();

        [Fact]
        public void CreateArray_CreatesArrayOfCorrectTypeAndLength()
        {
            // Arrange
            var elementType = typeof(int);
            var length = 5;

            // Act
            var array = this.fastCollectionFactory.CreateArray(elementType, length);

            // Assert
            array.Should().NotBeNull();
            array.Should().BeOfType<int[]>();
            array.Length.Should().Be(length);
        }

        [Fact]
        public void CreateArray_AllowsSettingValues()
        {
            // Arrange
            var elementType = typeof(string);
            var length = 3;

            // Act
            var array = this.fastCollectionFactory.CreateArray(elementType, length);
            array.SetValue("a", 0);
            array.SetValue("b", 1);
            array.SetValue("c", 2);

            // Assert
            array.Should().NotBeNull();
            array.Should().BeOfType<string[]>();
            ((string[])array).Should().ContainInOrder(new[] { "a", "b", "c" });
        }

        [Fact]
        public void CreateArray_ZeroLength_IsSupported()
        {
            // Arrange
            var elementType = typeof(Guid);
            var length = 0;

            // Act
            var array = this.fastCollectionFactory.CreateArray(elementType, length);

            // Assert
            array.Should().NotBeNull();
            ((Guid[])array).Should().BeEmpty();
        }

        [Fact]
        public void CreateArray_ReusesFactory_ForSameElementType()
        {
            // Arrange
            var elementType = typeof(int);

            // Act
            var first = this.fastCollectionFactory.CreateArray(elementType, 1);
            var second = this.fastCollectionFactory.CreateArray(elementType, 2);

            // Assert
            first.Should().NotBeNull();
            first.Should().BeOfType<int[]>();
            
            second.Should().NotBeNull();
            second.Should().BeOfType<int[]>();

            first.Should().NotBeSameAs(second);
        }

        [Fact]
        public void CreateList_CreatesListOfCorrectGenericType()
        {
            // Arrange
            var elementType = typeof(int);

            // Act
            var list = this.fastCollectionFactory.CreateList(elementType);

            // Assert
            list.Should().NotBeNull();
            list.Should().BeOfType<List<int>>();
            list.Should().BeAssignableTo<IList>();
        }

        [Fact]
        public void CreateList_AllowsAddingElements()
        {
            // Arrange
            var elementType = typeof(string);
            var list = (List<string>)this.fastCollectionFactory.CreateList(elementType);

            // Act
            list.Add("x");
            list.Add("y");

            // Assert
            list.Should().HaveCount(2);
            list.Should().ContainInOrder(new[] { "x", "y" });
        }

        [Fact]
        public void CreateList_ReusesFactory_ForSameElementType()
        {
            // Arrange
            var elementType = typeof(int);

            // Act
            var list1 = this.fastCollectionFactory.CreateList(elementType);
            var list2 = this.fastCollectionFactory.CreateList(elementType);

            // Assert
            list1.Should().NotBeNull();
            list1.Should().BeOfType<List<int>>();
            
            list2.Should().NotBeNull();
            list2.Should().BeOfType<List<int>>();

            list1.Should().NotBeSameAs(list2);
        }

        [Fact]
        public void CreateArray_DifferentElementTypes_CreateDifferentArrays()
        {
            // Arrange

            // Act
            var intArray = this.fastCollectionFactory.CreateArray(typeof(int), 1);
            var stringArray = this.fastCollectionFactory.CreateArray(typeof(string), 1);

            // Assert
            Assert.IsType<int[]>(intArray);
            Assert.IsType<string[]>(stringArray);
        }

        [Fact]
        public void CreateList_DifferentElementTypes_CreateDifferentLists()
        {
            // Act
            var intList = this.fastCollectionFactory.CreateList(typeof(int));
            var stringList = this.fastCollectionFactory.CreateList(typeof(string));

            // Assert
            intList.Should().BeOfType<List<int>>();
            stringList.Should().BeOfType<List<string>>();
        }

        [Fact]
        public void CreateArray_Throws_OnNullElementType()
        {
            // Arrange
            Action action = () => this.fastCollectionFactory.CreateArray(null!, 1);

            // Act
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateList_Throws_OnNullElementType()
        {
            // Arrange
            Action action = () => this.fastCollectionFactory.CreateList(null!);

            // Act
            action.Should().Throw<ArgumentNullException>();
        }
    }
}
