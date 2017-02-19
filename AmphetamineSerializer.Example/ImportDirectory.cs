using AmphetamineSerializer.Common;

namespace AmphetamineSerializer.Example
{
    public class ImportDirectory
    {
        [ASIndex(0)]
        public uint LookupTableRVA;
        [ASIndex(1)]
        public uint TimeStamp;
        [ASIndex(2)]
        public uint ForwarderChain;
        [ASIndex(3)]
        public uint NameRVA;
        [ASIndex(4)]
        public uint AddressRVA;
    }
}
