using BenchmarkDotNet.Running;
using NetSdr.Benchmarks.Benchmarks;

namespace NetSdr.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<NetSdrClientBenchmarks>();
    }
}
