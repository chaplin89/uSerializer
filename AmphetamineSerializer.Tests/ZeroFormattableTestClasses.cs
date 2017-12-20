using ZeroFormatter;

namespace AmphetamineSerializer.Tests
{
    [ZeroFormattable]
    public class ZeroTrivialTypes
    {
        [Index(0)] public virtual uint Test1_Field0 { get; set; }
        [Index(1)] public virtual int Test1_Field1 { get; set; }
        [Index(2)] public virtual ushort Test1_Field2 { get; set; }
        [Index(3)] public virtual short Test1_Field3 { get; set; }
        [Index(4)] public virtual sbyte Test1_Field4 { get; set; }
        [Index(5)] public virtual byte Test1_Field5 { get; set; }
        [Index(6)] public virtual string Test1_Field6 { get; set; }
    }
    
    [ZeroFormattable]
    public class ZeroTest1DArray
    {
        [Index(00)] public virtual uint Test1_Field0 {get;set;}
        [Index(01)] public virtual int Test1_Field1 {get;set;}
        [Index(02)] public virtual ushort Test1_Field2 {get;set;}
        [Index(03)] public virtual short Test1_Field3 {get;set;}
        [Index(04)] public virtual sbyte Test1_Field4 {get;set;}
        [Index(05)] public virtual byte Test1_Field5 {get;set;}
        [Index(06)] public virtual string Test1_Field6 {get;set;}

        [Index(07)] public virtual uint[] Test1_Field7 {get;set;}
        [Index(08)] public virtual int[] Test1_Field8 {get;set;}
        [Index(09)] public virtual ushort[] Test1_Field9 {get;set;}
        [Index(10)] public virtual short[] Test1_Field10 {get;set;}
        [Index(11)] public virtual sbyte[] Test1_Field11 {get;set;}
        [Index(12)] public virtual byte[] Test1_Field12 {get;set;}
        [Index(13)] public virtual string[] Test1_Field13 {get;set;}
    }

    [ZeroFormattable]
    public class ZeroTestJaggedArray
    {
        [Index(00)] public virtual uint Test1_Field0 {get;set;}
        [Index(01)] public virtual int Test1_Field1 {get;set;}
        [Index(02)] public virtual ushort Test1_Field2 {get;set;}
        [Index(03)] public virtual short Test1_Field3 {get;set;}
        [Index(04)] public virtual sbyte Test1_Field4 {get;set;}
        [Index(05)] public virtual byte Test1_Field5 {get;set;}
        [Index(06)] public virtual string Test1_Field6 {get;set;}

        [Index(07)] public virtual uint[] Test1_Field7 {get;set;}
        [Index(08)] public virtual int[] Test1_Field8 {get;set;}
        [Index(09)] public virtual ushort[] Test1_Field9 {get;set;}
        [Index(10)] public virtual short[] Test1_Field10 {get;set;}
        [Index(11)] public virtual sbyte[] Test1_Field11 {get;set;}
        [Index(12)] public virtual byte[] Test1_Field12 {get;set;}
        [Index(13)] public virtual string[] Test1_Field13 {get;set;}

        [Index(14)] public virtual uint[][] Test1_Field14 {get;set;}
        [Index(15)] public virtual int[][] Test1_Field15 {get;set;}
        [Index(16)] public virtual ushort[][] Test1_Field16 {get;set;}
        [Index(17)] public virtual short[][] Test1_Field17 {get;set;}
        [Index(18)] public virtual sbyte[][] Test1_Field18 {get;set;}
        [Index(19)] public virtual byte[][] Test1_Field19 {get;set;}
        [Index(20)] public virtual string[][] Test1_Field20 {get;set;}
    }

    [ZeroFormattable]
    public class ZeroTestFull
    {
        [Index(00)] public virtual uint Test_Field0 {get;set;}
        [Index(01)] public virtual int Test_Field1 {get;set;}
        [Index(02)] public virtual ushort Test_Field2 {get;set;}
        [Index(03)] public virtual short Test_Field3 {get;set;}
        [Index(04)] public virtual sbyte Test_Field4 {get;set;}
        [Index(05)] public virtual byte Test_Field5 {get;set;}
        [Index(06)] public virtual string Test_Field6 {get;set;}

        [Index(07)] public virtual uint[] Test_Field7 {get;set;}
        [Index(08)] public virtual int[] Test_Field8 {get;set;}
        [Index(09)] public virtual ushort[] Test_Field9 {get;set;}
        [Index(10)] public virtual short[] Test_Field10 {get;set;}
        [Index(11)] public virtual sbyte[] Test_Field11 {get;set;}
        [Index(12)] public virtual byte[] Test_Field12 {get;set;}
        [Index(13)] public virtual string[] Test_Field13 {get;set;}

        [Index(14)] public virtual uint[][] Test_Field14 {get;set;}
        [Index(15)] public virtual int[][] Test_Field15 {get;set;}
        [Index(16)] public virtual ushort[][] Test_Field16 {get;set;}
        [Index(17)] public virtual short[][] Test_Field17 {get;set;}
        [Index(18)] public virtual sbyte[][] Test_Field18 {get;set;}
        [Index(19)] public virtual byte[][] Test_Field19 {get;set;}
        [Index(20)] public virtual string[][] Test_Field20 {get;set;}

        [Index(21)] public virtual ZeroTestJaggedArray Test_Field21 {get;set;}
        [Index(22)] public virtual ZeroTestJaggedArray[] Test_Field22 {get;set;}
        [Index(23)] public virtual ZeroTestJaggedArray[][] Test_Field23 {get;set;}
    }
}
