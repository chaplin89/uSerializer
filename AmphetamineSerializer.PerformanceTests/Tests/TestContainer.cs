using AmphetamineSerializer.Tests;

namespace AmphetamineSerializer.PerformanceTests.Tests
{
    static class TestContainer
    {
        static public IPerformanceTest[] FieldTrivial
        {
            get
            {
                return new IPerformanceTest[]
                {
                    new AmphetamineTest<TestFieldTrivialTypes>(),
                    new BinaryFormatterTest<TestFieldTrivialTypes>(),
                    new DataContractTest<TestFieldTrivialTypes>(),
                    new XmlSerializerTest<TestFieldTrivialTypes>(),
                    new ZeroFormatterTest<ZeroTrivialTypes>()
                };
            }
        }

        static public IPerformanceTest[] Field1D
        {
            get
            {
                return new IPerformanceTest[]
                {
                    new AmphetamineTest<TestField1DArray>(),
                    new BinaryFormatterTest<TestField1DArray>(),
                    new DataContractTest<TestField1DArray>(),
                    new XmlSerializerTest<TestField1DArray>(),
                    new ZeroFormatterTest<ZeroTest1DArray>()
                };
            }
        }

        static public IPerformanceTest[] FieldJagged
        {
            get
            {
                return new IPerformanceTest[]
                {
                    new AmphetamineTest<TestFieldJaggedArray>(),
                    new BinaryFormatterTest<TestFieldJaggedArray>(),
                    new DataContractTest<TestFieldJaggedArray>(),
                    new XmlSerializerTest<TestFieldJaggedArray>(),
                    new ZeroFormatterTest<ZeroTestJaggedArray>()
                };
            }
        }

        static public IPerformanceTest[] FieldFull
        {
            get
            {
                return new IPerformanceTest[]
                {
                    new AmphetamineTest<TestFieldFull>(),
                    new BinaryFormatterTest<TestFieldFull>(),
                    new DataContractTest<TestFieldFull>(),
                    new XmlSerializerTest<TestFieldFull>(),
                    new ZeroFormatterTest<ZeroTestFull>()
                };
            }
        }
    }
}
