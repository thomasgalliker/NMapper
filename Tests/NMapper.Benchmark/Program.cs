using Benchmark;
using BenchmarkDotNet.Running;

namespace Diacritics.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<MapperBenchmark>();
            Console.WriteLine("Press any key to continue");
            Console.ReadLine();
        }
    }
}