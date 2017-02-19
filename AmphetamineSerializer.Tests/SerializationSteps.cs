using AmphetamineSerializer.Tests;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using TechTalk.SpecFlow;

namespace AmphetamineSerializer.SystemTests
{
    [Binding]
    public class SerializationSteps
    {
        Dictionary<string, object> stringInstanceMap = new Dictionary<string, object>();

        [Given(@"The instance (.*) of type (.*) filled with random data")]
        public void GivenTheClassTestFilledWithRandomData(string instance, string typeName)
        {
            Fixture fixture = new Fixture();
            Test test = fixture.Create<Test>();

            ScenarioContext.Current.Add(instance, test);
        }

        [Given(@"I serialize the instance (.*) of type (.*) in (.*)")]
        public void GivenISerializeTheClassTestInto(string instanceName, string type, string path)
        {
            Serializator<Test> serializator = new Serializator<Test>();
            var currentInstance = (Test)ScenarioContext.Current[instanceName];

            using (FileStream file = File.OpenWrite(path))
                serializator.Serialize(currentInstance, new BinaryWriter(file));
        }

        [When(@"I deserialize the instance (.*) of type (.*) from (.*)")]
        public void WhenIDeserializeTheClassTestIntoInstance(string instanceName, string type, string path)
        {
            Serializator<Test> deserializator = new Serializator<Test>();
            Test currentInstance = null;

            using (FileStream file = File.OpenRead(path))
                deserializator.Deserialize(ref currentInstance, new BinaryReader(file));

            ScenarioContext.Current.Add(instanceName, currentInstance);
        }

        [Then(@"(.*) and (.*) are identical")]
        public void ThenCompareInstanceAndInstanceReturnTrue(string inst1, string inst2)
        {
            Test obj1 = ScenarioContext.Current[inst1] as Test;
            Test obj2 = ScenarioContext.Current[inst2] as Test;
            using (var stream1 = new MemoryStream())
            using (var stream2 = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream1, obj1);
                formatter.Serialize(stream2, obj2);

                long position1 = stream1.Position;
                long position2 = stream2.Position;

                Debug.Assert(position1 == position2);

                stream1.Position = 0;
                stream2.Position = 0;

                for(int i=0; i< position1; i++)
                { 
                    int byte1 = stream1.ReadByte();
                    int byte2 = stream2.ReadByte();
                    Debug.Assert(byte1 == byte2);
                }
            }
        }

        private bool PublicFieldsAreEqual<T>(T self, T to) where T : class
        {
            if (self != null && to != null)
            {
                var type = typeof(T);
                var unequalProperties =
                    from pi in type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    let selfValue = type.GetField(pi.Name).GetValue(self)
                    let toValue = type.GetProperty(pi.Name).GetValue(to)
                    where !pi.FieldType.IsArray
                    where selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue))
                    select selfValue;

                if (unequalProperties.Any())
                    return false;
                var arrays =
                    from pi in type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    let selfValue = type.GetField(pi.Name).GetValue(self) as Array
                    let toValue = type.GetField(pi.Name).GetValue(to) as Array
                    where pi.FieldType.IsArray
                    select new { selfValue, toValue };

                foreach (var v in arrays)
                {
                    if (v.selfValue.Length != v.toValue.Length)
                        return false;

                    for (int i = 0; i < v.selfValue.Length; i++)
                    {
                        if (v.selfValue.GetValue(i) != v.toValue.GetValue(i))
                            return false;
                    }
                }
                return true;
            }
            return self == to;
        }
    }
}
