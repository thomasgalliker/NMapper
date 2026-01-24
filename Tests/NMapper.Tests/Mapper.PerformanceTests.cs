using System.Diagnostics;
using FluentAssertions;
using NMapper.TestData;
using Xunit;
using Xunit.Abstractions;

namespace NMapper.Tests
{
    public partial class MapperPerformanceTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public MapperPerformanceTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

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

            var stopwatch = Stopwatch.StartNew();

            // Act
            var target = mapper.Map<TargetWithCollections>(source);

            // Assert
            var elapsed = stopwatch.Elapsed;
            this.testOutputHelper.WriteLine($"stopwatch.Elapsed={elapsed.TotalMilliseconds}ms");
            target.Should().NotBeNull();
        }
    }
}
