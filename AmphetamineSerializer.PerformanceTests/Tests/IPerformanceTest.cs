using System.IO;

namespace AmphetamineSerializer.PerformanceTests
{
    interface IPerformanceTest
    {
        double Do(Stream stream);
        string Description { get; }
    }
}