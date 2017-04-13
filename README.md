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
[WIP]
## Extension
[WIP]
## Benchmark

Here follo the output of the benchmark inside AmphetamineSerializer.
Not much care has been put into trying to generate optimized code, it's not excluded that those results may vary considerably.

```
Number of iterations: 1000

++++++++++
Starting TestTrivialTypes
Mean time BinaryFormatter:  0,0118763
Mean time Amphetamine: 0,0010431
Mean time XmlSerializer: 0,0127014
++++++++++

++++++++++
Starting Test1DArray
Mean time BinaryFormatter:  0,0276328
Mean time Amphetamine: 0,00260779999999999
Mean time XmlSerializer: 0,0224132
++++++++++

++++++++++
Starting TestJaggedArray
Mean time BinaryFormatter:  0,0628334999999999
Mean time Amphetamine: 0,00628240000000002
Mean time XmlSerializer: 0,0520390000000001
++++++++++
```
