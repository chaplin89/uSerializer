using Ploeh.AutoFixture;
using System.IO;

namespace AmphetamineSerializer.PerformanceTests
{
    abstract class PerformanceTestBase<T> : IPerformanceTest
    {
        protected T graph;

        public PerformanceTestBase()
        {
            Fixture fixture = new Fixture();
            graph = fixture.Create<T>();
        }

        public abstract string Description { get; }

        public abstract double Do(Stream stream);
    }
}
