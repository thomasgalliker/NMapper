using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace NMapper.Tests.Logging
{
    public class TestOutputHelperLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper testOutputHelper;

        public TestOutputHelperLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputHelperLogger(this.testOutputHelper, categoryName);
        }

        public void Dispose()
        {
        }
    }

}