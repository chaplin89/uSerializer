using AmphetamineSerializer.Common;

namespace AmphetamineSerializer.Example
{
    public class DosHeader
    {
        [SIndex(0)]
        public ushort e_magic;
        [SIndex(1)]
        public ushort e_cblp;
        [SIndex(2)]
        public ushort e_cp;
        [SIndex(3)]
        public ushort e_crlc;
        [SIndex(4)]
        public ushort e_cparhdr;
        [SIndex(5)]
        public ushort e_minalloc;
        [SIndex(6)]
        public ushort e_maxalloc;
        [SIndex(7)]
        public ushort e_ss;
        [SIndex(8)]
        public ushort e_sp;
        [SIndex(9)]
        public ushort e_csum;
        [SIndex(10)]
        public ushort e_ip;
        [SIndex(11)]
        public ushort e_cs;
        [SIndex(12)]
        public ushort e_lfarlc;
        [SIndex(13)]
        public ushort e_ovno;
        [SIndex(14, ArrayFixedSize = 4)]
        public ushort[] e_res;
        [SIndex(15)]
        public ushort e_oemid;
        [SIndex(16)]
        public ushort e_oeminfo;
        [SIndex(17, ArrayFixedSize = 10)]
        public ushort[] e_res2;
        [SIndex(18)]
        public uint e_lfanew;
    }
}
