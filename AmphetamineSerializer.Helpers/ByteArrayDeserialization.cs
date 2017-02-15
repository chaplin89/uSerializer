using System;
using System.Text;

namespace AmphetamineSerializer.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    static public class ByteArrayDeserialization
    {
        static public void DecodeString(ref string decoded, byte[] toDeserialize, ref uint index, ulong options = 0)
        {
            if (toDeserialize[index + 4] == 0xFE)
                DecodeUnicodeString(ref decoded, toDeserialize, ref index);
            else
                DecodeASCIIString(ref decoded, toDeserialize, ref index);
        }

        static private void DecodeASCIIString(ref string decoded, byte[] toDeserialize, ref uint index, bool nullTerminated = false)
        {
            uint size;
            uint finalIndex = index;
            if (nullTerminated)
            {
                while (toDeserialize[finalIndex++] != 0x00) ;
                size = finalIndex - index;
            }
            else
            {
                size = BitConverter.ToUInt32(toDeserialize, (int)index);
            }

            if (size > 0)
                decoded = Encoding.ASCII.GetString(toDeserialize, (int)index + 4, (int)size);
            else
                decoded = string.Empty;
            index += 4 + size;
        }

        static private void DecodeUnicodeString(ref string decoded, byte[] toDeserialize, ref uint index, bool nullTerminated = false)
        {
            uint size;
            uint finalIndex = index;
            if (nullTerminated)
            {
                while (toDeserialize[finalIndex++] != 0x00) ;
                size = finalIndex - index;
            }
            else
            {
                size = BitConverter.ToUInt32(toDeserialize, (int)index);
            }

            if (size > 0)
            {
                decoded = Encoding.Unicode.GetString(toDeserialize, (int)index + 5, (int)(size - 1));
                index += 5 + (uint)size - 1;
            }
            else
            {
                decoded = string.Empty;
                size += 4;
            }
        }

        static public void DecodeInt(ref int decoded, byte[] toDeserialize, ref uint index, ulong options = 0)
        {
            decoded = BitConverter.ToInt32(toDeserialize, (int)index);
            index += 4;
        }
        
        static public void DecodeUInt(ref uint decoded, byte[] toDeserialize, ref uint index, ulong options = 0)
        {
            decoded = BitConverter.ToUInt32(toDeserialize, (int)index);
            index += 4;
        }
        
        static public void DecodeFloat(ref float decoded, byte[] toDeserialize, ref uint index, ulong options = 0)
        {
            decoded = BitConverter.ToSingle(toDeserialize, (int)index);
            index += 4;
        }
        
        static public void DecodeDouble(ref double decoded, byte[] toDeserialize, ref uint index, ulong options = 0)
        {
            decoded = BitConverter.ToDouble(toDeserialize, (int)index);
            index += 8;
        }

        static public void DecodeShort(ref short decoded, byte[] toDeserialize, ref uint index, ulong options = 0)
        {
            decoded = BitConverter.ToInt16(toDeserialize, (int)index);
            index += 2;
        }
        
        static public void DecodeUShort(ref ushort decoded, byte[] toDeserialize, ref uint index, ulong options = 0)
        {
            decoded = BitConverter.ToUInt16(toDeserialize, (int)index);
            index += 2;
        }
        
        static public void DecodeByte(ref byte decoded, byte[] toDeserialize, ref uint index, ulong options = 0)
        {
            decoded = toDeserialize[index++];
        }
    }
}
