using AmphetamineSerializer.Common;
using System;

namespace AmphetamineSerializer.Tests
{
    public class Test
    {
        [SIndex(0)]
        public uint Index;
        [SIndex(1)]
        public int Field1;
        [SIndex(2)]
        public ushort Field2;
        [SIndex(3)]
        public short Field3;
        [SIndex(4)]
        public sbyte Field4;
        [SIndex(5)]
        public byte Field5;
        [SIndex(6)]
        public string Field6;
        [SIndex(7)]
        public string[] Field7;
    }
}
