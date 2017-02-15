using AmphetamineSerializer;
using AmphetamineSerializer.Tests;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;

namespace Mermec.TrackWare.SystemTests
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
            TWSerializator<Test> serializator = new TWSerializator<Test>();
            var currentInstance = (Test)ScenarioContext.Current[instanceName];

            using (FileStream file = File.OpenWrite(path))
                serializator.Serialize(currentInstance, new BinaryWriter(file));            
        }

        [When(@"I deserialize the instance (.*) of type (.*) from (.*)")]
        public void WhenIDeserializeTheClassTestIntoInstance(string instanceName, string type, string path)
        {
            TWSerializator<Test> deserializator = new TWSerializator<Test>();
            Test currentInstance = null;

            using (FileStream file = File.OpenRead(path))
                deserializator.Deserialize(ref currentInstance, new BinaryReader(file));

            ScenarioContext.Current.Add(instanceName, currentInstance);
        }

        [Then(@"(.*) and (.*) are identical")]
        public void ThenCompareInstanceAndInstanceReturnTrue(string inst1, string inst2)
        {
            IEquatable<Test> obj1 = ScenarioContext.Current[inst1] as IEquatable<Test>;
            IEquatable<Test> obj2 = ScenarioContext.Current[inst2] as IEquatable<Test>;
            Debug.Assert(PublicFieldsAreEqual(obj1, obj2));
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
                    select new { selfValue, toValue};

                foreach(var v in arrays)
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
