using AmphetamineSerializer.Common;

namespace AmphetamineSerializer.Example
{
    public class ImportDirectory
    {
        [SIndex(0)]
        public uint LookupTableRVA;
        [SIndex(1)]
        public uint TimeStamp;
        [SIndex(2)]
        public uint ForwarderChain;
        [SIndex(3)]
        public uint NameRVA;
        [SIndex(4)]
        public uint AddressRVA;
    }
}
