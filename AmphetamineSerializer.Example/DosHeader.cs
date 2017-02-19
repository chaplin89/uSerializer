using AmphetamineSerializer.Common;

namespace AmphetamineSerializer.Example
{
    public class DosHeader
    {
        [ASIndex(0)]
        public ushort e_magic;
        [ASIndex(1)]
        public ushort e_cblp;
        [ASIndex(2)]
        public ushort e_cp;
        [ASIndex(3)]
        public ushort e_crlc;
        [ASIndex(4)]
        public ushort e_cparhdr;
        [ASIndex(5)]
        public ushort e_minalloc;
        [ASIndex(6)]
        public ushort e_maxalloc;
        [ASIndex(7)]
        public ushort e_ss;
        [ASIndex(8)]
        public ushort e_sp;
        [ASIndex(9)]
        public ushort e_csum;
        [ASIndex(10)]
        public ushort e_ip;
        [ASIndex(11)]
        public ushort e_cs;
        [ASIndex(12)]
        public ushort e_lfarlc;
        [ASIndex(13)]
        public ushort e_ovno;
        [ASIndex(14, ArrayFixedSize = 4)]
        public ushort[] e_res;
        [ASIndex(15)]
        public ushort e_oemid;
        [ASIndex(16)]
        public ushort e_oeminfo;
        [ASIndex(17, ArrayFixedSize = 10)]
        public ushort[] e_res2;
        [ASIndex(18)]
        public uint e_lfanew;
    }
}
