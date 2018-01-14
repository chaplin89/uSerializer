using AmphetamineSerializer.PerformanceTests.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AmphetamineSerializer.PerformanceTests
{
    class Program
    {
        const int totalIterations = 1000;

        static void Main(string[] args)
        {
            Console.WriteLine($"Number of iterations: {totalIterations}");
            Console.WriteLine();
            
            var performanceTrivial = TestSerialization(TestContainer.FieldTrivial);
            var performance1d = TestSerialization(TestContainer.Field1D);
            var performanceJagged = TestSerialization(TestContainer.FieldJagged);
            var performanceFull = TestSerialization(TestContainer.FieldFull);

            Console.ReadKey();
        }

        static Dictionary<string, double> TestSerialization(IPerformanceTest[] tests)
        {
            Dictionary<string, List<double>> performance = new Dictionary<string, List<double>>();

            foreach (var idx in Enumerable.Range(0, totalIterations + 1))
            {
                foreach (var testIndex in Enumerable.Range(0, tests.Length))
                {
                    using (var stream = new MemoryStream())
                    {
                        var elapsed = tests[testIndex].Do(stream);

                        if (idx == 0)
                            performance[tests[testIndex].Description] = new List<double>(totalIterations);
                        else
                            performance[tests[testIndex].Description].Add(elapsed);
                    }
                }
            }

            return performance.ToDictionary(_=> _.Key, _=> _.Value.Aggregate((_1, _2) => _1 + _2) / totalIterations);
        }
    }
}
