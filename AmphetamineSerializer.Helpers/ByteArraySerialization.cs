using System;
using System.Text;

namespace AmphetamineSerializer.Helpers
{
    /// <summary>
    /// Contain the logic for serializing/deserializing primitive types.
    /// </summary>
    static public class ByteArraySerialization
    {

        #region Serialization
        static public byte[] EncodeString(string toSerialize)
        {
            return EncodeUnicodeString(toSerialize as string);
        }

        static private byte[] EncodeASCIIString(string currentString)
        {
            int byteCount = Encoding.ASCII.GetByteCount(currentString);
            byte[] buffer = new byte[byteCount + 4];
            Encoding.ASCII.GetBytes(currentString, 0, currentString.Length, buffer, 4);
            BitConverter.GetBytes(byteCount).CopyTo(buffer, 0);
            return buffer;
        }

        static private byte[] EncodeUnicodeString(string currentString)
        {
            int byteCount = Encoding.Unicode.GetByteCount(currentString);
            if (byteCount > 0)
            {
                byte[] buffer = new byte[byteCount + 5];
                Encoding.Unicode.GetBytes(currentString, 0, currentString.Length, buffer, 5);
                BitConverter.GetBytes(byteCount).CopyTo(buffer, 0);
                buffer[4] = 0xFE;
                return buffer;
            }
            else
            {
                return BitConverter.GetBytes(byteCount);
            }
        }

        static public byte[] EncodeInt(int toSerialize)
        {
            return BitConverter.GetBytes(toSerialize);
        }

        static public byte[] EncodeUInt(uint toSerialize)
        {
            return BitConverter.GetBytes(toSerialize);
        }

        static public byte[] EncodeFloat(float toSerialize)
        {
            return BitConverter.GetBytes(toSerialize);
        }

        static public byte[] EncodeDouble(double toSerialize)
        {
            return BitConverter.GetBytes(toSerialize);
        }

        static public byte[] EncodeShort(short toSerialize)
        {
            return BitConverter.GetBytes(toSerialize);
        }

        static public byte[] EncodeUShort(ushort toSerialize)
        {
            return BitConverter.GetBytes(toSerialize);
        }

        static public byte[] EncodeByte(byte toSerialize)
        {
            return new byte[] { toSerialize };
        }

        static public byte[] EncodeSByte(sbyte toSerialize)
        {
            return new byte[] { (byte)toSerialize };
        }
        #endregion
    }
}
