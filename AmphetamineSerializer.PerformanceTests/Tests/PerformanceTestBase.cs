using Ploeh.AutoFixture;
using System;
using System.IO;

namespace AmphetamineSerializer.PerformanceTests
{
    abstract class PerformanceTestBase<T> : IPerformanceTest
    {
        private Fixture fixture;

        public PerformanceTestBase()
        {
            fixture = new Fixture();
        }

        public abstract string Description { get; }

        protected abstract double InternalDo(Stream stream, T graph);

        public double Do(Stream stream)
        {
            var res = InternalDo(stream, fixture.Create<T>());
            GC.Collect();
            return res;
        }
    }
}
