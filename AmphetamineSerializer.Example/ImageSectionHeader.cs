using AmphetamineSerializer.Common;
using System.Text;

namespace AmphetamineSerializer.Example
{
    public class ImageSectionHeader
    {
        [SIndex(0, ArrayFixedSize = 8)]
        public byte[] Name;
        [SIndex(1)]
        public uint Misc;
        [SIndex(2)]
        public uint VirtualAddress;
        [SIndex(3)]
        public uint SizeOfRawData;
        [SIndex(4)]
        public uint PointerToRawData;
        [SIndex(5)]
        public uint PointerToRelocations;
        [SIndex(6)]
        public uint PointerToLinenumbers;
        [SIndex(7)]
        public ushort NumberOfRelocations;
        [SIndex(8)]
        public ushort NumberOfLinenumbers;
        [SIndex(9)]
        public uint Characteristics;

        public string Name_
        {
            get
            {
                return Encoding.ASCII.GetString(Name);
            }
        }
    }
}
