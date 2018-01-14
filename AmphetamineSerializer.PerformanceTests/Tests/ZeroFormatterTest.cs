using System.Diagnostics;
using System.IO;
using ZeroFormatter;

namespace AmphetamineSerializer.PerformanceTests
{
    class ZeroFormatterTest<T> : PerformanceTestBase<T>
    {
        public ZeroFormatterTest()
        {
        }

        public override string Description => "ZeroFormatter";

        protected override double InternalDo(Stream stream, T graph)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ZeroFormatterSerializer.Serialize(stream, graph);
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }
    }
}
