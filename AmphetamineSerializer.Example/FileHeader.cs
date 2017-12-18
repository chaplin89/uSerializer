using AmphetamineSerializer.Model.Attributes;

namespace AmphetamineSerializer.Example
{
    public class FileHeader
    {
        [ASIndex(0)]
        public ushort Machine;
        [ASIndex(1)]
        public ushort NumberOfSections;
        [ASIndex(2)]
        public uint TimeDateStamp;
        [ASIndex(3)]
        public uint PointerToSymbolTable;
        [ASIndex(4)]
        public uint NumberOfSymbols;
        [ASIndex(5)]
        public ushort SizeOfOptionalHeader;
        [ASIndex(6)]
        public ushort Characteristics;
    }
}