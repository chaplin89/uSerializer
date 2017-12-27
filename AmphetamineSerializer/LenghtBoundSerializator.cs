using System;
using System.Diagnostics;
using System.IO;

namespace AmphetamineSerializer
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LengthBoundSerializator<T> : Serializator<T>
    {
        public LengthBoundSerializator()
        {
        }

        public override void Deserialize(ref T obj, BinaryReader stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.BaseStream.CanRead)
                throw new InvalidOperationException("The provided stream can't be read.");

            int initialLenght = stream.ReadInt32();
            int finalPosition = (int)stream.BaseStream.Position + initialLenght;

            base.Deserialize(ref obj, stream);

            if (stream.BaseStream.Position != finalPosition)
            {
                if (stream.BaseStream.Position > finalPosition)
                    throw new InvalidOperationException("Malformed stream.");

                if (!stream.BaseStream.CanSeek)
                {
                    byte[] buffer = null;
                    stream.BaseStream.Read(buffer, 0, finalPosition - (int)stream.BaseStream.Position);
                }
                else
                {
                    stream.BaseStream.Position = finalPosition;
                }
            }

            int lenght = stream.ReadInt32();
            Debug.Assert(lenght == initialLenght);
        }

        public override void Serialize(T obj, BinaryWriter stream)
        {
            if (!stream.BaseStream.CanWrite)
                throw new InvalidOperationException("The provided stream can't be read.");

            if (!stream.BaseStream.CanSeek)
                throw new InvalidOperationException("The provided stream can't be seeked.");

            uint lenght = 0;
            long initialPosition;
            long finalPosition;

            stream.Write(lenght);

            initialPosition = stream.BaseStream.Position;

            base.Serialize(obj, stream);

            finalPosition = stream.BaseStream.Position;

            lenght = (uint)(finalPosition - initialPosition);

            stream.Write(lenght);
            stream.BaseStream.Position = initialPosition - 4;
            stream.Write(lenght);
            stream.BaseStream.Position = finalPosition + 4;
        }
    }
}