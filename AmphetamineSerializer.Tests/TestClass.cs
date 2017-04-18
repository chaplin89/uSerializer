using AmphetamineSerializer.Common.Attributes;
using System;

namespace AmphetamineSerializer.Tests
{
        [Serializable]
    public class TestTrivialTypes
    {
        [ASIndex(0)]  public uint            Test1_Field0;
        [ASIndex(1)]  public int             Test1_Field1;
        [ASIndex(2)]  public ushort          Test1_Field2;
        [ASIndex(3)]  public short           Test1_Field3;
        [ASIndex(4)]  public sbyte           Test1_Field4;
        [ASIndex(5)]  public byte            Test1_Field5;
        [ASIndex(6)]  public string          Test1_Field6;
    }

    [Serializable]
    public class Test1DArray
    {
        [ASIndex(00)] public uint            Test1_Field0;
        [ASIndex(01)] public int             Test1_Field1;
        [ASIndex(02)] public ushort          Test1_Field2;
        [ASIndex(03)] public short           Test1_Field3;
        [ASIndex(04)] public sbyte           Test1_Field4;
        [ASIndex(05)] public byte            Test1_Field5;
        [ASIndex(06)] public string          Test1_Field6;

        [ASIndex(07)] public uint    []      Test1_Field7;
        [ASIndex(08)] public int     []      Test1_Field8;
        [ASIndex(09)] public ushort  []      Test1_Field9;
        [ASIndex(10)] public short   []      Test1_Field10;
        [ASIndex(11)] public sbyte   []      Test1_Field11;
        [ASIndex(12)] public byte    []      Test1_Field12;
        [ASIndex(13)] public string  []      Test1_Field13;
    }

    [Serializable]
    public class TestJaggedArray
    {
        [ASIndex(00)] public uint            Test1_Field0;
        [ASIndex(01)] public int             Test1_Field1;
        [ASIndex(02)] public ushort          Test1_Field2;
        [ASIndex(03)] public short           Test1_Field3;
        [ASIndex(04)] public sbyte           Test1_Field4;
        [ASIndex(05)] public byte            Test1_Field5;
        [ASIndex(06)] public string          Test1_Field6;

        [ASIndex(07)] public uint    []      Test1_Field7;
        [ASIndex(08)] public int     []      Test1_Field8;
        [ASIndex(09)] public ushort  []      Test1_Field9;
        [ASIndex(10)] public short   []      Test1_Field10;
        [ASIndex(11)] public sbyte   []      Test1_Field11;
        [ASIndex(12)] public byte    []      Test1_Field12;
        [ASIndex(13)] public string  []      Test1_Field13;
        
        [ASIndex(14)] public uint    [][]    Test1_Field14;
        [ASIndex(15)] public int     [][]    Test1_Field15;
        [ASIndex(16)] public ushort  [][]    Test1_Field16;
        [ASIndex(17)] public short   [][]    Test1_Field17;
        [ASIndex(18)] public sbyte   [][]    Test1_Field18;
        [ASIndex(19)] public byte    [][]    Test1_Field19;
        [ASIndex(20)] public string  [][]    Test1_Field20;
    }

    [Serializable]
    public class TestFull
    {
        [ASIndex(00)] public uint            Test_Field0;
        [ASIndex(01)] public int             Test_Field1;
        [ASIndex(02)] public ushort          Test_Field2;
        [ASIndex(03)] public short           Test_Field3;
        [ASIndex(04)] public sbyte           Test_Field4;
        [ASIndex(05)] public byte            Test_Field5;
        [ASIndex(06)] public string          Test_Field6;

        [ASIndex(07)] public uint    []      Test_Field7;
        [ASIndex(08)] public int     []      Test_Field8;
        [ASIndex(09)] public ushort  []      Test_Field9;
        [ASIndex(10)] public short   []      Test_Field10;
        [ASIndex(11)] public sbyte   []      Test_Field11;
        [ASIndex(12)] public byte    []      Test_Field12;
        [ASIndex(13)] public string  []      Test_Field13;
        
        [ASIndex(14)] public uint    [][]    Test_Field14;
        [ASIndex(15)] public int     [][]    Test_Field15;
        [ASIndex(16)] public ushort  [][]    Test_Field16;
        [ASIndex(17)] public short   [][]    Test_Field17;
        [ASIndex(18)] public sbyte   [][]    Test_Field18;
        [ASIndex(19)] public byte    [][]    Test_Field19;
        [ASIndex(20)] public string  [][]    Test_Field20;

        [ASIndex(21)] public TestJaggedArray           Test_Field21;
        [ASIndex(22)] public TestJaggedArray   []      Test_Field22;
        [ASIndex(23)] public TestJaggedArray   [][]    Test_Field23;
    }

    [Serializable]
    public class TestFullVersion
    {
        [ASIndex(00)] public int                               Version;
        [ASIndex(01, VersionBegin=100)] public int             Test_Field1;
        [ASIndex(02, VersionBegin=100)] public ushort          Test_Field2;
        [ASIndex(03, VersionBegin=100)] public short           Test_Field3;
        [ASIndex(04, VersionBegin=100)] public sbyte           Test_Field4;
        [ASIndex(05, VersionBegin=100)] public byte            Test_Field5;
        [ASIndex(06, VersionBegin=100)] public string          Test_Field6;

        [ASIndex(07, VersionBegin=101)] public uint    []      Test_Field7;
        [ASIndex(08, VersionBegin=101)] public int     []      Test_Field8;
        [ASIndex(09, VersionBegin=101)] public ushort  []      Test_Field9;
        [ASIndex(10, VersionBegin=101)] public short   []      Test_Field10;
        [ASIndex(11, VersionBegin=101)] public sbyte   []      Test_Field11;
        [ASIndex(12, VersionBegin=101)] public byte    []      Test_Field12;
        [ASIndex(13, VersionBegin=101)] public string  []      Test_Field13;
        
        [ASIndex(14, VersionBegin=102)] public uint    [][]    Test_Field14;
        [ASIndex(15, VersionBegin=102)] public int     [][]    Test_Field15;
        [ASIndex(16, VersionBegin=102)] public ushort  [][]    Test_Field16;
        [ASIndex(17, VersionBegin=102)] public short   [][]    Test_Field17;
        [ASIndex(18, VersionBegin=102)] public sbyte   [][]    Test_Field18;
        [ASIndex(19, VersionBegin=102)] public byte    [][]    Test_Field19;
        [ASIndex(20, VersionBegin=102)] public string  [][]    Test_Field20;

        [ASIndex(21)] public TestJaggedArray           Test_Field21;
        [ASIndex(22)] public TestJaggedArray   []      Test_Field22;
        [ASIndex(23)] public TestJaggedArray   [][]    Test_Field23;
    }
}