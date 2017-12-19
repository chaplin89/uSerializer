# AmphetamineSerializer
[![Build status](https://ci.appveyor.com/api/projects/status/jbsqh4a686ost3mc?svg=true)](https://ci.appveyor.com/project/chaplin89/amphetamineserializer)
[![Test status](http://teststatusbadge.azurewebsites.net/api/status/chaplin89/AmphetamineSerializer)](https://ci.appveyor.com/project/chaplin89/amphetamineserializer)

It's a binary serializator that is made to ease the interoperation between managed and native code.
It makes porting an existing C/C++ structure to managed code very easy. Most of the time, all you need to do is to copy the structure and decorate its fields.

**This is WIP, definitely not suitable for production**

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

Please note that the version field must come before any other field and due to some limitation of the attributes in C#, it can be only numerical or string but at the moment only numbers are supported. 

There are plans to support the version field to be a generic complex object.
## Extension
[WIP]
## Benchmark

Here follow the output of the benchmark inside AmphetamineSerializer that show how AmpethamineSerializer performance compare to the performance of other serializators.
Not much care has been put into trying to generate optimized code, it's not excluded that those results may vary considerably.

(The less, the better)

**NOTE**: this test only serialization on a MemoryStream. Of course, this does not take into account a lot of things.
   ![Trivial](/Charts/1_Trivial.png)
   ![1D](/Charts/2_1DArray.png)
   ![Jagged](/Charts/3_Jagged.png)
   ![Full](/Charts/4_Full.png)
