using AmphetamineSerializer.Common;
using System;

namespace AmphetamineSerializer.Tests
{
    [Serializable]
    public class Test
    {
        [ASIndex(0)]
        public uint Index;
        [ASIndex(1)]
        public int Field1;
        [ASIndex(2)]
        public ushort Field2;
        [ASIndex(3)]
        public short Field3;
        [ASIndex(4)]
        public sbyte Field4;
        [ASIndex(5)]
        public byte Field5;
        [ASIndex(6)]
        public string Field6;
        //[ASIndex(7)]
        //public string[] Field7;
    }
}
