using AmphetamineSerializer.Common;
using AmphetamineSerializer.Model.Attributes;
using System;
using System.Text;

namespace AmphetamineSerializer.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    static public class ByteArray
    {
        [SerializationHandler(typeof(string))]
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
        
        [SerializationHandler(typeof(byte))]
        [SerializationHandler(typeof(sbyte))]
        [SerializationHandler(typeof(uint))]
        [SerializationHandler(typeof(int))]
        [SerializationHandler(typeof(ushort))]
        [SerializationHandler(typeof(short))]
        [SerializationHandler(typeof(double))]
        [SerializationHandler(typeof(float))]
        static public bool DecodePrimitive(Context ctx)
        {
            return false;
        }
    }
}
