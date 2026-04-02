using System.Collections;
using System.Collections.ObjectModel;
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
        public void CreateCollection_CreatesListOfCorrectGenericType()
        {
            // Arrange
            var elementType = typeof(int);

            // Act
            var list = this.fastCollectionFactory.CreateCollection(typeof(List<int>), elementType);

            // Assert
            list.Collection.Should().NotBeNull();
            list.Collection.Should().BeOfType<List<int>>();
            list.Collection.Should().BeAssignableTo<IList>();
        }

        [Fact]
        public void CreateCollection_AllowsAddingElements()
        {
            // Arrange
            var elementType = typeof(string);
            var collection = this.fastCollectionFactory.CreateCollection(typeof(List<string>), elementType);

            // Act
            collection.Add("x");
            collection.Add("y");

            // Assert
            var list = (List<string>)collection.Collection;
            list.Should().HaveCount(2);
            list.Should().ContainInOrder(new[] { "x", "y" });
        }

        [Fact]
        public void CreateCollection_ReusesFactory_ForSameCollectionType()
        {
            // Arrange
            var elementType = typeof(int);

            // Act
            var list1 = this.fastCollectionFactory.CreateCollection(typeof(List<int>), elementType);
            var list2 = this.fastCollectionFactory.CreateCollection(typeof(List<int>), elementType);

            // Assert
            list1.Collection.Should().NotBeNull();
            list1.Collection.Should().BeOfType<List<int>>();
            
            list2.Collection.Should().NotBeNull();
            list2.Collection.Should().BeOfType<List<int>>();

            list1.Collection.Should().NotBeSameAs(list2.Collection);
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
        public void CreateCollection_DifferentElementTypes_CreateDifferentLists()
        {
            // Act
            var intList = this.fastCollectionFactory.CreateCollection(typeof(List<int>), typeof(int));
            var stringList = this.fastCollectionFactory.CreateCollection(typeof(List<string>), typeof(string));

            // Assert
            intList.Collection.Should().BeOfType<List<int>>();
            stringList.Collection.Should().BeOfType<List<string>>();
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
        public void CreateCollection_Throws_OnNullElementType()
        {
            // Arrange
            Action action = () => this.fastCollectionFactory.CreateCollection(typeof(List<string>), null!);

            // Act
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CreateCollection_CreatesHashSet_ForSetInterface()
        {
            // Act
            var collection = this.fastCollectionFactory.CreateCollection(typeof(ISet<int>), typeof(int));
            collection.Add(1);
            collection.Add(2);

            // Assert
            collection.Collection.Should().BeOfType<HashSet<int>>();
            ((HashSet<int>)collection.Collection).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public void CreateCollection_CreatesCollection_ForConcreteCollectionType()
        {
            // Act
            var collection = this.fastCollectionFactory.CreateCollection(typeof(Collection<string>), typeof(string));
            collection.Add("a");
            collection.Add("b");

            // Assert
            collection.Collection.Should().BeOfType<Collection<string>>();
            ((Collection<string>)collection.Collection).Should().ContainInOrder("a", "b");
        }
    }
}
