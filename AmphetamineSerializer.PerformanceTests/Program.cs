using AmphetamineSerializer.Tests;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace AmphetamineSerializer.PerformanceTests
{
    class Program
    {
        const int totalIterations = 1000;

        static void Main(string[] args)
        {
            Console.WriteLine($"Number of iterations: {totalIterations}");
            Console.WriteLine();
            TestSerialization<TestTrivialTypes>();
            TestSerialization<Test1DArray>();
            TestSerialization<TestJaggedArray>();
            TestSerialization<TestFull>();
            Console.ReadKey();
        }

        static void TestSerialization<T>()
        {
            Console.WriteLine(new string('+', 10));
            Console.WriteLine($"Starting {typeof(T).Name}");
            List<double> s1Performance = new List<double>();
            List<double> s2Performance = new List<double>();
            List<double> s3Performance = new List<double>();

            BinaryFormatter serializer1 = new BinaryFormatter();
            Serializator<T> serializer2 = new Serializator<T>();
            XmlSerializer serializer3 = new XmlSerializer(typeof(T));

            foreach (var idx in Enumerable.Range(1, totalIterations))
            {
                using (var stream1 = new MemoryStream())
                using (var stream2 = new BinaryWriter(new MemoryStream()))
                using (var stream3 = new MemoryStream())
                {

                    Fixture fixture = new Fixture();
                    T toSerialize = fixture.Create<T>();

                    Stopwatch t1 = new Stopwatch();
                    Stopwatch t2 = new Stopwatch();
                    Stopwatch t3 = new Stopwatch();

                    t1.Start();
                    serializer1.Serialize(stream1, toSerialize);
                    t1.Stop();

                    t2.Start();
                    serializer2.Serialize(toSerialize, stream2);
                    t2.Stop();

                    t3.Start();
                    serializer3.Serialize(stream3, toSerialize);
                    t3.Stop();

                    if (idx != 1)
                    {
                        s1Performance.Add(t1.Elapsed.TotalMilliseconds);
                        s2Performance.Add(t2.Elapsed.TotalMilliseconds);
                        s3Performance.Add(t3.Elapsed.TotalMilliseconds);
                    }

                    if (t2.ElapsedMilliseconds > t3.ElapsedMilliseconds)
                    {
                        Trace.WriteLine("true");
                    }
                }
            }
            double time1 = s1Performance.Aggregate((_1, _2) => _1 + _2) / totalIterations;
            double time2 = s2Performance.Aggregate((_1, _2) => _1 + _2) / totalIterations;
            double time3 = s3Performance.Aggregate((_1, _2) => _1 + _2) / totalIterations;

            Console.WriteLine($"Mean time BinaryFormatter:  {time1}");
            Console.WriteLine($"Mean time Amphetamine: {time2}");
            Console.WriteLine($"Mean time XmlSerializer: {time3}");
            Console.WriteLine(new string('+', 10));
            Console.WriteLine();
        }
    }
}
