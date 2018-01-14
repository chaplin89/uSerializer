using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace AmphetamineSerializer.PerformanceTests
{
    class DataContractTest<T> : PerformanceTestBase<T>
    {
        private DataContractSerializer serializer;

        public DataContractTest()
        {
            serializer = new DataContractSerializer(typeof(T));
        }

        public override string Description => "DataContract";

        protected override double InternalDo(Stream stream, T graph)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            serializer.WriteObject(stream, graph);
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }
    }
}
