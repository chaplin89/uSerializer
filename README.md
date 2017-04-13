# AmphetamineSerializer
[![Build status](https://ci.appveyor.com/api/projects/status/jbsqh4a686ost3mc?svg=true)](https://ci.appveyor.com/project/chaplin89/amphetamineserializer)

It's a binary serializator that is made to ease the interoperation between managed and native code.
It makes porting an existing C/C++ structure to managed code very easy. Most of the time, all you need to do is to copy the structure and decorate its fields.

**This is WIP, definitely not suitable for production**

## Features
* **Performance** It's capable of generating ad-hoc assemblies for serialization. This will provide an overall good performance.
* **Control** It allows a very precise control on the binary format that the serializator supports.
* **Extensibility** The library on its own provide some support for basic binary formats but you can integrate the support for a custom binary format in different ways. The library provie some facilities to do this directly in IL or other higher-level languages.
* **Versioning** It allow support for serializing/deserializing different version of the same structure.

## How to use
[WIP]
### Versioning
[WIP]
## Extension
[WIP]
## Benchmark
[WIP]
