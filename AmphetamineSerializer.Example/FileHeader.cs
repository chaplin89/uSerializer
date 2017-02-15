using AmphetamineSerializer.Common;

namespace AmphetamineSerializer.Example
{
    public class FileHeader
    {
        [SIndex(0)]
        public ushort Machine;
        [SIndex(1)]
        public ushort NumberOfSections;
        [SIndex(2)]
        public uint TimeDateStamp;
        [SIndex(3)]
        public uint PointerToSymbolTable;
        [SIndex(4)]
        public uint NumberOfSymbols;
        [SIndex(5)]
        public ushort SizeOfOptionalHeader;
        [SIndex(6)]
        public ushort Characteristics;
    }
}