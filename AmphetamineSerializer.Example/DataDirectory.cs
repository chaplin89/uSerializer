using AmphetamineSerializer.Common;

namespace AmphetamineSerializer.Example
{
    public class DataDirectory
    {

        [SIndex(0)]
        public uint VirtualAddress;
        [SIndex(1)]
        public uint Size;
    }
}
