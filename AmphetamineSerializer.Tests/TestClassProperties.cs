using AmphetamineSerializer.Model.Attributes;
using System;
using System.Runtime.Serialization;

namespace AmphetamineSerializer.Tests
{
    [Serializable, DataContract]
    public class TestPropertyTrivialTypes
    {
        [ASIndex(0), DataMember] public uint Test_Property0 { get; set; }
        [ASIndex(1), DataMember] public int Test_Property1 { get; set; }
        [ASIndex(2), DataMember] public ushort Test_Property2 { get; set; }
        [ASIndex(3), DataMember] public short Test_Property3 { get; set; }
        [ASIndex(4), DataMember] public sbyte Test_Property4 { get; set; }
        [ASIndex(5), DataMember] public byte Test_Property5 { get; set; }
        [ASIndex(6), DataMember] public string Test_Property6 { get; set; }
    }

    [Serializable, DataContract]
    public class TestProperty1DArray
    {
        [ASIndex(00), DataMember] public uint Test_Property0 { get; set; }
        [ASIndex(01), DataMember] public int Test_Property1 { get; set; }
        [ASIndex(02), DataMember] public ushort Test_Property2 { get; set; }
        [ASIndex(03), DataMember] public short Test_Property3 { get; set; }
        [ASIndex(04), DataMember] public sbyte Test_Property4 { get; set; }
        [ASIndex(05), DataMember] public byte Test_Property5 { get; set; }
        [ASIndex(06), DataMember] public string Test_Property6 { get; set; }

        [ASIndex(07), DataMember] public uint[] Test_Property7 { get; set; }
        [ASIndex(08), DataMember] public int[] Test_Property8 { get; set; }
        [ASIndex(09), DataMember] public ushort[] Test_Property9 { get; set; }
        [ASIndex(10), DataMember] public short[] Test_Property10 { get; set; }
        [ASIndex(11), DataMember] public sbyte[] Test_Property11 { get; set; }
        [ASIndex(12), DataMember] public byte[] Test_Property12 { get; set; }
        [ASIndex(13), DataMember] public string[] Test_Property13 { get; set; }
    }

    [Serializable, DataContract]
    public class TestPropertyJaggedArray
    {
        [ASIndex(00), DataMember] public uint Test_Property0 { get; set; }
        [ASIndex(01), DataMember] public int Test_Property1 { get; set; }
        [ASIndex(02), DataMember] public ushort Test_Property2 { get; set; }
        [ASIndex(03), DataMember] public short Test_Property3 { get; set; }
        [ASIndex(04), DataMember] public sbyte Test_Property4 { get; set; }
        [ASIndex(05), DataMember] public byte Test_Property5 { get; set; }
        [ASIndex(06), DataMember] public string Test_Property6 { get; set; }

        [ASIndex(07), DataMember] public uint[] Test_Property7 { get; set; }
        [ASIndex(08), DataMember] public int[] Test_Property8 { get; set; }
        [ASIndex(09), DataMember] public ushort[] Test_Property9 { get; set; }
        [ASIndex(10), DataMember] public short[] Test_Property10 { get; set; }
        [ASIndex(11), DataMember] public sbyte[] Test_Property11 { get; set; }
        [ASIndex(12), DataMember] public byte[] Test_Property12 { get; set; }
        [ASIndex(13), DataMember] public string[] Test_Property13 { get; set; }

        [ASIndex(14), DataMember] public uint[][] Test_Property14 { get; set; }
        [ASIndex(15), DataMember] public int[][] Test_Property15 { get; set; }
        [ASIndex(16), DataMember] public ushort[][] Test_Property16 { get; set; }
        [ASIndex(17), DataMember] public short[][] Test_Property17 { get; set; }
        [ASIndex(18), DataMember] public sbyte[][] Test_Property18 { get; set; }
        [ASIndex(19), DataMember] public byte[][] Test_Property19 { get; set; }
        [ASIndex(20), DataMember] public string[][] Test_Property20 { get; set; }
    }

    [Serializable, DataContract]
    public class TestPropertyFull
    {
        [ASIndex(00), DataMember] public uint Test_Property0 { get; set; }
        [ASIndex(01), DataMember] public int Test_Property1 { get; set; }
        [ASIndex(02), DataMember] public ushort Test_Property2 { get; set; }
        [ASIndex(03), DataMember] public short Test_Property3 { get; set; }
        [ASIndex(04), DataMember] public sbyte Test_Property4 { get; set; }
        [ASIndex(05), DataMember] public byte Test_Property5 { get; set; }
        [ASIndex(06), DataMember] public string Test_Property6 { get; set; }

        [ASIndex(07), DataMember] public uint[] Test_Property7 { get; set; }
        [ASIndex(08), DataMember] public int[] Test_Property8 { get; set; }
        [ASIndex(09), DataMember] public ushort[] Test_Property9 { get; set; }
        [ASIndex(10), DataMember] public short[] Test_Property10 { get; set; }
        [ASIndex(11), DataMember] public sbyte[] Test_Property11 { get; set; }
        [ASIndex(12), DataMember] public byte[] Test_Property12 { get; set; }
        [ASIndex(13), DataMember] public string[] Test_Property13 { get; set; }

        [ASIndex(14), DataMember] public uint[][] Test_Property14 { get; set; }
        [ASIndex(15), DataMember] public int[][] Test_Property15 { get; set; }
        [ASIndex(16), DataMember] public ushort[][] Test_Property16 { get; set; }
        [ASIndex(17), DataMember] public short[][] Test_Property17 { get; set; }
        [ASIndex(18), DataMember] public sbyte[][] Test_Property18 { get; set; }
        [ASIndex(19), DataMember] public byte[][] Test_Property19 { get; set; }
        [ASIndex(20), DataMember] public string[][] Test_Property20 { get; set; }

        [ASIndex(21), DataMember] public TestJaggedArray Test_Property21 { get; set; }
        [ASIndex(22), DataMember] public TestJaggedArray[] Test_Property22 { get; set; }
        [ASIndex(23), DataMember] public TestJaggedArray[][] Test_Property23 { get; set; }
    }

    [Serializable, DataContract]
    public class TestPropertyFullVersion
    {
        [ASIndex(00), DataMember] public int Version { get; set; }

        [ASIndex(02, Version = 100), DataMember] public float Test_100 { get; set; }
        [ASIndex(03, Version = 101), DataMember] public double Test_101 { get; set; }
        [ASIndex(04, Version = 102), DataMember] public Contained_102 Test_102 { get; set; }

        // [ASIndex(21)] public TestJaggedArray           Test_Property21{get;set;}
        // [ASIndex(22)] public TestJaggedArray   []      Test_Property22{get;set;}
        // [ASIndex(23)] public TestJaggedArray   [][]    Test_Property23{get;set;}
    }

    [Serializable, DataContract]
    public class PropertyContained_100
    {
        [ASIndex(01)] public int Test_Property1 { get; set; }
        [ASIndex(02)] public ushort Test_Property2 { get; set; }
        [ASIndex(03)] public short Test_Property3 { get; set; }
        [ASIndex(04)] public sbyte Test_Property4 { get; set; }
        [ASIndex(05)] public byte Test_Property5 { get; set; }
        [ASIndex(06)] public string Test_Property6 { get; set; }
    }

    [Serializable, DataContract]
    public class PropertyContained_101
    {
        [ASIndex(01)] public uint[] Test_Property7 { get; set; }
        [ASIndex(02)] public int[] Test_Property8 { get; set; }
        [ASIndex(03)] public ushort[] Test_Property9 { get; set; }
        [ASIndex(04)] public short[] Test_Property10 { get; set; }
        [ASIndex(05)] public sbyte[] Test_Property11 { get; set; }
        [ASIndex(06)] public byte[] Test_Property12 { get; set; }
        [ASIndex(07)] public string[] Test_Property13 { get; set; }
    }

    [Serializable, DataContract]
    public class PropertyContained_102
    {
        [ASIndex(01)] public uint[][] Test_Property14 { get; set; }
        [ASIndex(02)] public int[][] Test_Property15 { get; set; }
        [ASIndex(03)] public ushort[][] Test_Property16 { get; set; }
        [ASIndex(04)] public short[][] Test_Property17 { get; set; }
        [ASIndex(05)] public sbyte[][] Test_Property18 { get; set; }
        [ASIndex(06)] public byte[][] Test_Property19 { get; set; }
        [ASIndex(07)] public string[][] Test_Property20 { get; set; }
    }
}
