# AmphetamineSerializer
[![Build status](https://ci.appveyor.com/api/projects/status/jbsqh4a686ost3mc?svg=true)](https://ci.appveyor.com/project/chaplin89/amphetamineserializer)
[![Test status](http://teststatusbadge.azurewebsites.net/api/status/chaplin89/AmphetamineSerializer)](https://ci.appveyor.com/project/chaplin89/amphetamineserializer)

It's a serializator that is made to ease the interoperation between managed and native code.
It makes porting an existing C/C++ structure to managed code very easy. Most of the time, all you need to do is to copy the structure and decorate its fields.

**This is WIP, definitely not suitable for production**

## Table of contents
  * [Features](#features)
  * [How to use](#how-to-use)
     * [Serialization example](#serialization-example)
     * [Deserialization example](#deserialization-example)
     * [Versioning](#versioning)
  * [TODO](#todo-not-in-any-specific-order)
  * [Benchmark](#benchmark)

## Features
* **Performance** It's capable of generating ad-hoc assemblies for serialization. This will provide an overall good performance.
* **Control** It allows a very precise control on the binary format that the serializator supports.
* **Extensibility** The library on its own provide some support for basic binary formats but you can integrate the support for a custom binary format in different ways. The library provie some facilities to do this directly in IL or other higher-level languages.
* **Versioning** It allow support for serializing/deserializing different version of the same structure.

## How to use
Given the class MyClass, defined as follow:
```csharp
public class MyClass
{
    [ASIndex(00)] public uint            Test_Field0;
    [ASIndex(01)] public int             Test_Field1;
    [ASIndex(02)] public ushort          Test_Field2;
    [ASIndex(03)] public short           Test_Field3;
    [ASIndex(04)] public sbyte           Test_Field4;
    [ASIndex(05)] public byte            Test_Field5;
    [ASIndex(06)] public string          Test_Field6;
}
```

### Serialization example

```csharp
MyClass myClass = new MyClass();
var stream = new MemoryStream();
var serializator = new Serializator<MyClass>();

// Fill myClass
[...]

serializator.Serialize(myClass, stream);
```

### Deserialization example

```csharp
// Get an existing stream
MyClass myClass = null;
var stream = ...;
var serializator = new Serializator<MyClass>();
serializatior.Deserialize(ref myClass, stream);
```
### Versioning
AmphetamineSerializer allow to define multiple version of the same structure.

Say you have the version 1 of your sample, declared like this:
```csharp
public class MyClass
{
    [ASIndex(00)] public uint            Version;
    [ASIndex(01)] public int             Test_Field1;
    [ASIndex(02)] public ushort          Test_Field2;
}
```
Then you change your packet to the version 2, in order to add the field Test_Field3 at the end. 

Of course if you simply add the field at the end there'll be a catch: the serializer will end up trying to serialize or deserialize the field regardless the version number. 

In this case, you can define the structure like this:
```csharp
public class MyClass
{
    [ASIndex(00)] public uint              Version;
    [ASIndex(01)] public int               Test_Field1;
    [ASIndex(02)] public ushort            Test_Field2;
    [ASIndex(02, Version=2)] public ushort Test_Field3;
}
```
The first three field will be always serialized/deserialized, but the Test_Field3 field will be serialized only if the Version is 2.

Please note that the version field must come before any other field and due to some limitation of the attributes in C#, it can be only a  numerical or a string type but at the moment only numbers are supported. 

There are plans to support the version field to be a generic complex object.
## Extension
[WIP]
## TODO (not in any specific order)
- [X] ~~Remove obsolete code/improve readability~~
- [X] ~~Test coverage of the versioning part~~
- [X] ~~Provide a better abstraction for the IL generation part~~
- [X] ~~Allow an array to have its lenght in any other field~~ (miss tests coverage)
- [X] ~~Think about/implement something to manage properties other than fields~~ (tests coverage for complex object fail)
- [ ] ~~Implement byte array backend~~ (in progress)
- [ ] Document all classes
- [ ] Compile to multiple .NET Framework
- [ ] Create a NuGet package
- [ ] Allow version to be a complex object
- [ ] Improve the example in order to support serialization/deserialization of a full PE32/PE64 Header
- [ ] Provide a better support for plugin-like features
- [ ] Support for null object
- [ ] Support for struct
- [ ] Implement other backends
## Benchmark

Here follow the output of the benchmark inside AmphetamineSerializer that show how AmpethamineSerializer performance compare to the performance of other serializators.

**Note 1**: Performance is not a top priority at this stage of development. Actually, most of the efforts are moving toward defining a semi-decent architecture with a reasonable level of maintainability (as far as writing IL code can be defined "maintainable") and developing unit-tests, so this chart is purely indicative because there are room for improvements for sure.

**Note 2**: It's simply impossible to provide a completely fair comparison. Of course, some of those serializator are binary, other are textual; some use bytearray, some don't. Nevertheless this can be an interesting starting point to see how AS compare to other serializators.

(In ms; the less, the better)

   ![Trivial](/Charts/1_Trivial.png)
   ![1D](/Charts/2_1DArray.png)
   ![Jagged](/Charts/3_Jagged.png)
   ![Full](/Charts/4_Full.png)
