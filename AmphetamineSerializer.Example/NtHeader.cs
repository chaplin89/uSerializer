using AmphetamineSerializer.Model.Attributes;

namespace AmphetamineSerializer.Example
{
    public class NtHeader
    {
        [ASIndex(0)]
        public uint Signature;
        [ASIndex(1)]
        public FileHeader FileHeader;
        [ASIndex(2)]
        public OptionalHeader32 OptionalHeader;
    }
}
