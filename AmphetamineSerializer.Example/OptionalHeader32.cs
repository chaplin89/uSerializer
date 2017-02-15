using AmphetamineSerializer.Common;

namespace AmphetamineSerializer.Example
{
    public class OptionalHeader32
    {
        [SIndex(0)]
        public ushort Magic;
        [SIndex(1)]
        public byte MajorLinkerVersion;
        [SIndex(2)]
        public byte MinorLinkerVersion;
        [SIndex(3)]
        public uint SizeOfCode;
        [SIndex(4)]
        public uint SizeOfInitializedData;
        [SIndex(5)]
        public uint SizeOfUninitializedData;
        [SIndex(6)]
        public uint AddressOfEntryPoint;
        [SIndex(7)]
        public uint BaseOfCode;
        [SIndex(8)]
        public uint BaseOfData;
        [SIndex(9)]
        public uint ImageBase;
        [SIndex(10)]
        public uint SectionAlignment;
        [SIndex(11)]
        public uint FileAlignment;
        [SIndex(12)]
        public ushort MajorOperatingSystemVersion;
        [SIndex(13)]
        public ushort MinorOperatingSystemVersion;
        [SIndex(14)]
        public ushort MajorImageVersion;
        [SIndex(15)]
        public ushort MinorImageVersion;
        [SIndex(16)]
        public ushort MajorSubsystemVersion;
        [SIndex(17)]
        public ushort MinorSubsystemVersion;
        [SIndex(18)]
        public uint Win32VersionValue;
        [SIndex(19)]
        public uint SizeOfImage;
        [SIndex(20)]
        public uint SizeOfHeaders;
        [SIndex(21)]
        public uint CheckSum;
        [SIndex(22)]
        public ushort Subsystem;
        [SIndex(23)]
        public ushort DllCharacteristics;
        [SIndex(24)]
        public uint SizeOfStackReserve;
        [SIndex(25)]
        public uint SizeOfStackCommit;
        [SIndex(26)]
        public uint SizeOfHeapReserve;
        [SIndex(27)]
        public uint SizeOfHeapCommit;
        [SIndex(28)]
        public uint LoaderFlags;
        [SIndex(29)]
        public uint NumberOfRvaAndSizes;
        [SIndex(29, ArrayFixedSize = 16)]
        public DataDirectory[] DataDirectory;
    }
}
