using AmphetamineSerializer.Interfaces;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using TechTalk.SpecFlow;

namespace AmphetamineSerializer.Tests
{
    [Binding]
    public class SerializationSteps
    {
        [Given(@"The instance (.*) of type (.*) filled with random data")]
        public void GivenTheClassTestFilledWithRandomData(string instance, string typeName)
        {
            Type type = Assembly.GetExecutingAssembly()
                                .ExportedTypes
                                .Where(x => x.Name == typeName)
                                .Single();
            Assert.NotNull(type);

            object randomData = CreateNonVersion(type);

            if (type.Name == "TestFieldFullVersion")
                randomData = CreateVersion();

            ScenarioContext.Current.Add("typeToSerialize", type);
            ScenarioContext.Current.Add(instance, randomData);
        }

        private object CreateNonVersion(Type type)
        {
            Fixture fixture = new Fixture();
            var method = typeof(SpecimenFactory)
                        .GetMethods()
                        .Where(_ => _.Name == "Create")
                        .Where(_ => _.GetParameters().Length == 1)
                        .Where(_ => _.GetParameters().First().ParameterType.IsAssignableFrom(typeof(ISpecimenBuilder)))
                        .Single()
                        .MakeGenericMethod(type);

            return method.Invoke(null, new object[] { fixture });
        }

        object CreateVersion()
        {
            TestFieldFullVersion randomData = new TestFieldFullVersion()
            {
                Version = 102,
                Test_102 = (FieldContained_102)CreateNonVersion(typeof(FieldContained_102))
            };

            return randomData;
        }

        [Given(@"I serialize the instance (.*) in (.*)")]
        public void GivenISerializeTheInstanceInto(string objectInstance, string streamInstance)
        {
            var currentInstance = ScenarioContext.Current[objectInstance];
            var type = (Type)ScenarioContext.Current["typeToSerialize"];
            var serializator = GetSerializator(type);
            var stream = new MemoryStream();

            ScenarioContext.Current[streamInstance] = stream;
            serializator.Serialize(currentInstance, new BinaryWriter(stream));
        }

        private ISerializator GetSerializator(Type currentInstance)
        {
            Type closedType = typeof(Serializator<>).MakeGenericType(currentInstance);
            return (ISerializator)Activator.CreateInstance(closedType, new object[] { null }, null);
        }

        [When(@"I deserialize the instance (.*) from (.*)")]
        public void WhenIDeserializeTheInstanceFrom(string objectInstance, string streamInstance)
        {
            var type = (Type)ScenarioContext.Current["typeToSerialize"];
            var serializator = GetSerializator(type);

            var stream = (MemoryStream)ScenarioContext.Current[streamInstance];

            object newInstance = null;

            stream.Position = 0;
            serializator.Deserialize(ref newInstance, new BinaryReader(stream));

            ScenarioContext.Current[objectInstance] = newInstance;
            stream.Dispose();
        }

        [Then(@"(.*) and (.*) are identical")]
        public void ThenInstancesAreIdentical(string inst1, string inst2)
        {
            object obj1 = ScenarioContext.Current[inst1];
            object obj2 = ScenarioContext.Current[inst2];
            using (var stream1 = new MemoryStream())
            using (var stream2 = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream1, obj1);
                formatter.Serialize(stream2, obj2);

                long position1 = stream1.Position;
                long position2 = stream2.Position;

                Assert.NotZero(position1);
                Assert.NotZero(position2);
                Assert.AreEqual(position1, position2);

                stream1.Position = 0;
                stream2.Position = 0;

                for (int i = 0; i < position1; i++)
                {
                    int byte1 = stream1.ReadByte();
                    int byte2 = stream2.ReadByte();
                    Assert.AreEqual(byte1, byte2);
                }
            }
        }
    }
}
