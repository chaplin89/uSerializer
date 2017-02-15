using AmphetamineSerializer.Common;

namespace AmphetamineSerializer.Example
{
    public class NtHeader
    {
        [SIndex(0)]
        public uint Signature;
        [SIndex(1)]
        public FileHeader FileHeader;
        [SIndex(2)]
        public OptionalHeader32 OptionalHeader;
    }
}
