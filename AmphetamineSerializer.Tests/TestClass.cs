using AmphetamineSerializer.Common;
using System;

namespace AmphetamineSerializer.Tests
{

    public class Test1
    {
        [ASIndex(0)] public uint Index;
        [ASIndex(1)] public int Field1;
        [ASIndex(2)] public ushort Field2;
        [ASIndex(3)] public short Field3;
        [ASIndex(4)] public sbyte Field4;
        [ASIndex(5)] public byte Field5;
        [ASIndex(6)] public string Field6;

        [ASIndex(7)] public uint[] Field7;
        [ASIndex(8)] public int[] Field8;
        [ASIndex(9)] public ushort[] Field9;
        [ASIndex(10)] public short[] Field10;
        [ASIndex(11)] public sbyte[] Field11;
        [ASIndex(12)] public byte[] Field12;
        [ASIndex(13)] public string[] Field13;
    }

    [Serializable]
    public class Test
    {
        [ASIndex(0)] public uint Index;
        [ASIndex(1)] public int Field1;
        [ASIndex(2)] public ushort Field2;
        [ASIndex(3)] public short Field3;
        [ASIndex(4)] public sbyte Field4;
        [ASIndex(5)] public byte Field5;
        [ASIndex(6)] public string Field6;

        [ASIndex(7)] public uint[] Field7;
        [ASIndex(8)] public int[] Field8;
        [ASIndex(9)] public ushort[] Field9;
        [ASIndex(10)] public short[] Field10;
        [ASIndex(11)] public sbyte[] Field11;
        [ASIndex(12)] public byte[] Field12;
        [ASIndex(13)] public string[] Field13;

        [ASIndex(13)] public Test1 Field14;
    }
}
