using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AmphetamineSerializer.PerformanceTests
{
    class BinaryFormatterTest<T> : PerformanceTestBase<T>
    {
        private BinaryFormatter serializer;

        public BinaryFormatterTest()
        {
            serializer = new BinaryFormatter();
        }

        public override string Description => "BinaryFormatter";

        protected override double InternalDo(Stream stream, T graph)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            serializer.Serialize(stream, graph);
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }
    }
}
