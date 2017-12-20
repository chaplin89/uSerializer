using AmphetamineSerializer.Tests;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using ZeroFormatter;
using ZeroFormatter.Formatters;

namespace AmphetamineSerializer.PerformanceTests
{

    class Program
    {
        const int totalIterations = 1000;

        static void Main(string[] args)
        {
            Console.WriteLine($"Number of iterations: {totalIterations}");
            Console.WriteLine();
            TestSerialization<TestTrivialTypes, ZeroTrivialTypes>();
            TestSerialization<Test1DArray, ZeroTest1DArray>();
            TestSerialization<TestJaggedArray, ZeroTestJaggedArray>();
            TestSerialization<TestFull, ZeroTestFull>();
            Console.ReadKey();
        }

        static void TestSerialization<T, V>()
        {
            Console.WriteLine(new string('+', 10));
            Console.WriteLine($"Starting {typeof(T).Name}");

            // TODO: remove this shit
            List<double> s1Performance = new List<double>();
            List<double> s2Performance = new List<double>();
            List<double> s3Performance = new List<double>();
            List<double> s4Performance = new List<double>();
            List<double> s5Performance = new List<double>();

            BinaryFormatter serializer1 = new BinaryFormatter();
            Serializator<T> serializer2 = new Serializator<T>();
            XmlSerializer serializer3 = new XmlSerializer(typeof(T));
            DataContractSerializer serializer4 = new DataContractSerializer(typeof(T));


            foreach (var idx in Enumerable.Range(1, totalIterations))
            {
                using (var stream1 = new MemoryStream())
                using (var stream2 = new BinaryWriter(new MemoryStream()))
                using (var stream3 = new MemoryStream())
                using (var stream4 = new MemoryStream())
                using (var stream5 = new MemoryStream())
                {

                    Fixture fixture = new Fixture();
                    T toSerialize = fixture.Create<T>();
                    V zeroToSerialize = fixture.Create<V>();

                    Stopwatch t1 = new Stopwatch();
                    Stopwatch t2 = new Stopwatch();
                    Stopwatch t3 = new Stopwatch();
                    Stopwatch t4 = new Stopwatch();
                    Stopwatch t5 = new Stopwatch();

                    t1.Start();
                    serializer1.Serialize(stream1, toSerialize);
                    t1.Stop();

                    t2.Start();
                    serializer2.Serialize(toSerialize, stream2);
                    t2.Stop();

                    t3.Start();
                    serializer3.Serialize(stream3, toSerialize);
                    t3.Stop();

                    t4.Start();
                    serializer4.WriteObject(stream4, toSerialize);
                    t4.Stop();
                                        
                    t5.Start();
                    ZeroFormatterSerializer.Serialize(stream5, zeroToSerialize);
                    t5.Stop();

                    if (idx != 1)
                    {
                        s1Performance.Add(t1.Elapsed.TotalMilliseconds);
                        s2Performance.Add(t2.Elapsed.TotalMilliseconds);
                        s3Performance.Add(t3.Elapsed.TotalMilliseconds);
                        s4Performance.Add(t4.Elapsed.TotalMilliseconds);
                        s5Performance.Add(t5.Elapsed.TotalMilliseconds);
                    }
                }
            }

            double[] meanTime = new double[5];
            double[] percentage = new double[5];
            string[] messages = new string[]
            {
                "Mean time BinaryFormatter:",
                "Mean time Amphetamine:",
                "Mean time XmlSerializer:",
                "Mean time DataContractSerializer:",
                "Mean time ZeroFormatter:"
            };

            meanTime[0] = s1Performance.Aggregate((_1, _2) => _1 + _2) / totalIterations;
            meanTime[1] = s2Performance.Aggregate((_1, _2) => _1 + _2) / totalIterations;
            meanTime[2] = s3Performance.Aggregate((_1, _2) => _1 + _2) / totalIterations;
            meanTime[3] = s4Performance.Aggregate((_1, _2) => _1 + _2) / totalIterations;
            meanTime[4] = s5Performance.Aggregate((_1, _2) => _1 + _2) / totalIterations;

            var min = meanTime.Max();

            percentage[0] = meanTime[0] / min;
            percentage[1] = meanTime[1] / min;
            percentage[2] = meanTime[2] / min;
            percentage[3] = meanTime[3] / min;
            percentage[4] = meanTime[4] / min;

            var beginPosition = messages.Select(x => x.Length).Max() + 1;
            var endPosition = Console.BufferWidth - 5;

            var blockLenght = (endPosition - beginPosition) / percentage.Max();

            List<string> bars = new List<string>();

            foreach (var v in percentage)
            {//█
                bars.Add(new string('X', (int)Math.Floor(blockLenght * v) - 3));
            }

            for (int i = 0; i < messages.Length; i++)
            {
                Console.Write(messages[i]);
                //Console.CursorLeft = beginPosition;
                Console.WriteLine(meanTime[i]);
            }

            //Console.WriteLine($"Mean time BinaryFormatter:  {percentage[0]}");
            //Console.WriteLine($"Mean time Amphetamine: {percentage[1]}");
            //Console.WriteLine($"Mean time XmlSerializer: {percentage[2]}");
            Console.WriteLine(new string('+', 10));
            Console.WriteLine();
        }
    }
}
