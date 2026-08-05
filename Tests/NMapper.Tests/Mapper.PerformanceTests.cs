namespace NMapper.Tests
{
    public partial class MapperTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public MapperTests(ITestOutputHelper testOutputHelper)
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
