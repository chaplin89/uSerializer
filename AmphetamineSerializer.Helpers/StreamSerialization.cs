using System.IO;
using System.Text;

namespace AmphetamineSerializer.Helpers
{
    static public class StreamSerialization
    {
        static public void EncodeString(string toSerialize, BinaryWriter stream)
        {
            EncodeUnicodeString(toSerialize, stream);
        }

        static private void EncodeASCIIString(string currentString, BinaryWriter stream)
        {
            int byteCount = Encoding.ASCII.GetByteCount(currentString);
            stream.Write(byteCount);
            if (byteCount > 0)
            {
                stream.Write(Encoding.ASCII.GetBytes(currentString));
            }
        }

        static private void EncodeUnicodeString(string currentString, BinaryWriter stream)
        {
            int byteCount = 1;
            if (!string.IsNullOrEmpty(currentString))
                byteCount += Encoding.Unicode.GetByteCount(currentString);
            byte controlChar = 0xFE;
            stream.Write(byteCount);
            stream.Write(controlChar);
            if (byteCount > 1)
                stream.Write(Encoding.Unicode.GetBytes(currentString));
        }

        static public void EncodeInt(int toSerialize, BinaryWriter stream)
        {
            stream.Write(toSerialize);
        }

        static public void EncodeUInt(uint toSerialize, BinaryWriter stream)
        {
            stream.Write(toSerialize);
        }

        static public void EncodeFloat(float toSerialize, BinaryWriter stream)
        {
            stream.Write(toSerialize);
        }

        static public void EncodeDouble(double toSerialize, BinaryWriter stream)
        {
            stream.Write(toSerialize);
        }

        static public void EncodeShort(short toSerialize, BinaryWriter stream)
        {
            stream.Write(toSerialize);
        }

        static public void EncodeUShort(ushort toSerialize, BinaryWriter stream)
        {
            stream.Write(toSerialize);
        }

        static public void EncodeByte(byte toSerialize, BinaryWriter stream)
        {
            stream.Write(toSerialize);
        }

        static public void EncodeSByte(sbyte toSerialize, BinaryWriter stream)
        {
            stream.Write(toSerialize);
        }
    }
}
