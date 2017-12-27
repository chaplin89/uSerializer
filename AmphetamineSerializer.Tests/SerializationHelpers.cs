using NUnit.Framework;
using Ploeh.AutoFixture;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TechTalk.SpecFlow;

namespace AmphetamineSerializer.Tests
{
    internal interface ISerializationHelpers
    {
        void GenerateRandomImpl(string instance);
        void SerializeIntoImpl(string objectInstance, string streamInstance);
        void SerializeFromImpl(string objectInstance, string streamInstance);
        void EnsureAreEqualImpl(string inst1, string inst2);
    }

    internal abstract class SerializationHelpersBase<ClassType> : ISerializationHelpers
    {
        public void SerializeIntoImpl(string objectInstance, string streamInstance)
        {
            var serializator = new Serializator<ClassType>();
            var currentInstance = (ClassType)ScenarioContext.Current[objectInstance];
            var stream = new MemoryStream();

            ScenarioContext.Current[streamInstance] = stream;
            serializator.Serialize(currentInstance, new BinaryWriter(stream));
        }

        public void SerializeFromImpl(string objectInstance, string streamInstance)
        {
            var deserializator = new Serializator<ClassType>();
            var stream = (MemoryStream)ScenarioContext.Current[streamInstance];

            ClassType currentInstance = default(ClassType);

            stream.Position = 0;
            deserializator.Deserialize(ref currentInstance, new BinaryReader(stream));

            ScenarioContext.Current[objectInstance] = currentInstance;
            stream.Dispose();
        }

        public void EnsureAreEqualImpl(string inst1, string inst2)
        {
            ClassType obj1 = (ClassType)ScenarioContext.Current[inst1];
            ClassType obj2 = (ClassType)ScenarioContext.Current[inst2];
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

        public abstract void GenerateRandomImpl(string instance);
    }

    internal class SerializationVersionHelpers : SerializationHelpersBase<TestFullVersion>
    {
        public override void GenerateRandomImpl(string instance)
        {
            TestFullVersion fv = new TestFullVersion();
            Fixture fixture = new Fixture();

            fv.Version = 102;
            fv.Test_102 = fixture.Create<Contained_102>();

            ScenarioContext.Current.Add(instance, fv);
        }
    }

    internal class SerializationHelpers<ClassType> : SerializationHelpersBase<ClassType>
    {
        public override void GenerateRandomImpl(string instance)
        {
            Fixture fixture = new Fixture();
            ClassType test = fixture.Create<ClassType>();

            ScenarioContext.Current.Add(instance, test);
        }
    }
}
