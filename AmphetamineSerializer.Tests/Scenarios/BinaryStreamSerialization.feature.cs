﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.2.0.0
//      SpecFlow Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace AmphetamineSerializer.Tests.Scenarios
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.2.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("BinaryStreamSerialization")]
    public partial class BinaryStreamSerializationFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "BinaryStreamSerialization.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "BinaryStreamSerialization", "\tSerialization and deserialization of the same class\r\n\tin order to ensure that re" +
                    "sults are consistent.", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Serializing and deserializing an element containing 1D arrays data produce the sa" +
            "me result")]
        public virtual void SerializingAndDeserializingAnElementContaining1DArraysDataProduceTheSameResult()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Serializing and deserializing an element containing 1D arrays data produce the sa" +
                    "me result", ((string[])(null)));
#line 5
this.ScenarioSetup(scenarioInfo);
#line 6
 testRunner.Given("The instance Instance1 of type Test1DArray filled with random data", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 7
 testRunner.And("I serialize the instance Instance1 in Stream1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 8
 testRunner.When("I deserialize the instance Instance2 from Stream1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 9
 testRunner.Then("Instance1 and Instance2 are identical", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Serializing and deserializing complex nested types produce the same result")]
        public virtual void SerializingAndDeserializingComplexNestedTypesProduceTheSameResult()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Serializing and deserializing complex nested types produce the same result", ((string[])(null)));
#line 11
this.ScenarioSetup(scenarioInfo);
#line 12
 testRunner.Given("The instance Instance1 of type TestFull filled with random data", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 13
 testRunner.And("I serialize the instance Instance1 in Stream1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 14
 testRunner.When("I deserialize the instance Instance2 from Stream1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 15
 testRunner.Then("Instance1 and Instance2 are identical", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Serializing and deserializing containing jagged array produce the same result")]
        public virtual void SerializingAndDeserializingContainingJaggedArrayProduceTheSameResult()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Serializing and deserializing containing jagged array produce the same result", ((string[])(null)));
#line 17
this.ScenarioSetup(scenarioInfo);
#line 18
 testRunner.Given("The instance Instance1 of type TestJaggedArray filled with random data", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 19
 testRunner.And("I serialize the instance Instance1 in Stream1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 20
 testRunner.When("I deserialize the instance Instance2 from Stream1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 21
 testRunner.Then("Instance1 and Instance2 are identical", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Serializing and deserializing trivial types produce the same result")]
        public virtual void SerializingAndDeserializingTrivialTypesProduceTheSameResult()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Serializing and deserializing trivial types produce the same result", ((string[])(null)));
#line 23
this.ScenarioSetup(scenarioInfo);
#line 24
 testRunner.Given("The instance Instance1 of type TestTrivialTypes filled with random data", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 25
 testRunner.And("I serialize the instance Instance1 in Stream1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 26
 testRunner.When("I deserialize the instance Instance2 from Stream1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
 testRunner.Then("Instance1 and Instance2 are identical", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Serializing and deserializing complex nested types with version produce the same " +
            "result")]
        public virtual void SerializingAndDeserializingComplexNestedTypesWithVersionProduceTheSameResult()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Serializing and deserializing complex nested types with version produce the same " +
                    "result", ((string[])(null)));
#line 29
this.ScenarioSetup(scenarioInfo);
#line 30
 testRunner.Given("The instance Instance1 of type TestFullVersion filled with random data", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 31
 testRunner.And("I serialize the instance Instance1 in Stream1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 32
 testRunner.When("I deserialize the instance Instance2 from Stream1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 33
 testRunner.Then("Instance1 and Instance2 are identical", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
