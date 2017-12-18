using AmphetamineSerializer.Common;
using AmphetamineSerializer.Model.Attributes;
using System.Text;

namespace AmphetamineSerializer.Example
{
    public class ImageSectionHeader
    {
        [ASIndex(0, ArrayFixedSize = 8)]
        public byte[] Name;
        [ASIndex(1)]
        public uint Misc;
        [ASIndex(2)]
        public uint VirtualAddress;
        [ASIndex(3)]
        public uint SizeOfRawData;
        [ASIndex(4)]
        public uint PointerToRawData;
        [ASIndex(5)]
        public uint PointerToRelocations;
        [ASIndex(6)]
        public uint PointerToLinenumbers;
        [ASIndex(7)]
        public ushort NumberOfRelocations;
        [ASIndex(8)]
        public ushort NumberOfLinenumbers;
        [ASIndex(9)]
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
