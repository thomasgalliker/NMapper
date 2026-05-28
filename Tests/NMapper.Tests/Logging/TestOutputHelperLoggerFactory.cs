namespace NMapper.Tests.Logging
{
    public class TestOutputHelperLoggerFactory : ILoggerFactory
    {
        private readonly ITestOutputHelper testOutputHelper;

        public TestOutputHelperLoggerFactory(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        }

        public ILogger CreateLogger(string categoryName)
        {
            ArgumentNullException.ThrowIfNull(categoryName);

            return new TestOutputHelperLogger(this.testOutputHelper, categoryName);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);
        }

        public void Dispose()
        {
        }
    }
}