using System.IO;
using System.Text;

namespace AmphetamineSerializer.Helpers
{
    static public class StreamDeserialization
    {
        static public void DecodeString(ref string decoded, BinaryReader stream)
        {
            int size = stream.ReadInt32();
            int controlChar = stream.ReadByte();
            if ((controlChar & 0xff) == 0xfe)
            {
                DecodeUnicodeString(ref decoded, stream, size - 1);
            }
            else
            {
                stream.BaseStream.Position--;
                DecodeASCIIString(ref decoded, stream, size);
            }
        }

        static private void DecodeASCIIString(ref string decoded, BinaryReader stream, int size)
        {
            byte[] buffer = stream.ReadBytes(size);
            if (size > 0)
                decoded = Encoding.ASCII.GetString(buffer, 0, size);
            else
                decoded = string.Empty;
        }

        static private void DecodeUnicodeString(ref string decoded, BinaryReader stream, int size)
        {
            byte[] buffer = stream.ReadBytes(size);

            if (size > 0)
            {
                decoded = Encoding.Unicode.GetString(buffer, 0, size);
            }
            else
            {
                decoded = string.Empty;
            }
        }

        static public void DecodeInt(ref int decoded, BinaryReader stream)
        {
            decoded = stream.ReadInt32();
        }

        static public void DecodeUInt(ref uint decoded, BinaryReader stream)
        {
            decoded = stream.ReadUInt32();
        }

        static public void DecodeFloat(ref float decoded, BinaryReader stream)
        {
            decoded = stream.ReadSingle();
        }

        static public void DecodeDouble(ref double decoded, BinaryReader stream)
        {
            decoded = stream.ReadDouble();
        }

        static public void DecodeShort(ref short decoded, BinaryReader stream)
        {
            decoded = stream.ReadInt16();
        }

        static public void DecodeUShort(ref ushort decoded, BinaryReader stream)
        {
            decoded = stream.ReadUInt16();
        }

        static public void DecodeByte(ref byte decoded, BinaryReader stream)
        {
            decoded = stream.ReadByte();
        }

        static public void DecodeSByte(ref sbyte decoded, BinaryReader stream)
        {
            decoded = stream.ReadSByte();
        }
    }
}
