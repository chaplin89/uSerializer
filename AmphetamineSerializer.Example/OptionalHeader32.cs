using AmphetamineSerializer.Common;
using AmphetamineSerializer.Model.Attributes;

namespace AmphetamineSerializer.Example
{
    public class OptionalHeader32
    {
        [ASIndex(0)]
        public ushort Magic;
        [ASIndex(1)]
        public byte MajorLinkerVersion;
        [ASIndex(2)]
        public byte MinorLinkerVersion;
        [ASIndex(3)]
        public uint SizeOfCode;
        [ASIndex(4)]
        public uint SizeOfInitializedData;
        [ASIndex(5)]
        public uint SizeOfUninitializedData;
        [ASIndex(6)]
        public uint AddressOfEntryPoint;
        [ASIndex(7)]
        public uint BaseOfCode;
        [ASIndex(8)]
        public uint BaseOfData;
        [ASIndex(9)]
        public uint ImageBase;
        [ASIndex(10)]
        public uint SectionAlignment;
        [ASIndex(11)]
        public uint FileAlignment;
        [ASIndex(12)]
        public ushort MajorOperatingSystemVersion;
        [ASIndex(13)]
        public ushort MinorOperatingSystemVersion;
        [ASIndex(14)]
        public ushort MajorImageVersion;
        [ASIndex(15)]
        public ushort MinorImageVersion;
        [ASIndex(16)]
        public ushort MajorSubsystemVersion;
        [ASIndex(17)]
        public ushort MinorSubsystemVersion;
        [ASIndex(18)]
        public uint Win32VersionValue;
        [ASIndex(19)]
        public uint SizeOfImage;
        [ASIndex(20)]
        public uint SizeOfHeaders;
        [ASIndex(21)]
        public uint CheckSum;
        [ASIndex(22)]
        public ushort Subsystem;
        [ASIndex(23)]
        public ushort DllCharacteristics;
        [ASIndex(24)]
        public uint SizeOfStackReserve;
        [ASIndex(25)]
        public uint SizeOfStackCommit;
        [ASIndex(26)]
        public uint SizeOfHeapReserve;
        [ASIndex(27)]
        public uint SizeOfHeapCommit;
        [ASIndex(28)]
        public uint LoaderFlags;
        [ASIndex(29)]
        public uint NumberOfRvaAndSizes;
        [ASIndex(29, ArrayFixedSize = 16)]
        public DataDirectory[] DataDirectory;
    }
}
