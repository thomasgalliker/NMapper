using FluentAssertions;
using NMapper.TestData;
using Xunit;

namespace NMapper.Tests
{
    public partial class MapperPerformanceTests
    {
        [Fact]
        public void ShouldMap_SourceToTargetCollection()
        {
            // Arrange
            var mappings = new IMapping[]
            {
                new SourceToTargetCollectionsMapping(),
                new ItemMapping(),
                new ListItemMapping()
            };
            IMapper mapper = new Mapper(mappings);

            var source = SourceWithCollectionsHelper.CreateSource(1000000);

            // Act
            var target = mapper.Map<TargetWithCollections>(source);

            // Assert
            target.Should().NotBeNull();
        }
    }
}
