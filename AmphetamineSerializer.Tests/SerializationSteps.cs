﻿using AmphetamineSerializer.Tests;
using NUnit.Framework;
using Ploeh.AutoFixture;
using System;
using System.Collections.Generic;
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
        ISerializationHelpers helpers;


        Dictionary<string, object> stringInstanceMap = new Dictionary<string, object>();
        

        [Given(@"The instance (.*) of type (.*) filled with random data")]
        public void GivenTheClassTestFilledWithRandomData(string instance, string typeName)
        {
            Type type = Assembly.GetExecutingAssembly()
                                .ExportedTypes
                                .Where(x=> x.Name == typeName)
                                .Single();
            Assert.NotNull(type);

            var closedType = typeof(SerializationHelpers<>).MakeGenericType(type);
            helpers = (ISerializationHelpers)Activator.CreateInstance(closedType);
            helpers.GenerateRandomImpl(instance);
        }

        [Given(@"I serialize the instance (.*) in (.*)")]
        public void GivenISerializeTheInstanceInto(string objectInstance, string streamInstance)
        {
            helpers.SerializeIntoImpl(objectInstance, streamInstance);
        }

        [When(@"I deserialize the instance (.*) from (.*)")]
        public void WhenIDeserializeTheInstanceFrom(string objectInstance, string streamInstance)
        {
            helpers.SerializeFromImpl(objectInstance, streamInstance);
        }

        [Then(@"(.*) and (.*) are identical")]
        public void ThenInstancesAreIdentical(string inst1, string inst2)
        {
            helpers.EnsureAreEqualImpl(inst1, inst2);
        }
    }
}
