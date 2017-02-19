using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;
using System;
using System.Diagnostics;
using System.IO;

namespace AmphetamineSerializer
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LenghtBoundSerializator<T> : Serializator<T>
    {
        delegate void DeserializeBytes(ref T obj, byte[] buffer, ref uint position);
        delegate void SerializeBinaryWriter(ref T obj, BinaryWriter stream);
        delegate void DeserializeBinaryReader(ref T obj, BinaryReader stream);

        DeserializeBytes deserializeFromBytes;
        DeserializeBinaryReader deserializeFromStream;
        SerializeBinaryWriter serializeFromStream;

        private object additionalContext;
        IChainManager chain = ChainManager.MakeDefaultChain();

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="buffer"></param>
        /// <param name="position"></param>
        public override void Deserialize(ref T obj, byte[] buffer, ref uint position)
        {
            position += 4;
            deserializeFromBytes(ref obj, buffer, ref position);
            position += 4;
        }

        public override void Deserialize(ref T obj, BinaryReader stream)
        {
            int initialLenght = stream.ReadInt32();
            int finalPosition = (int)stream.BaseStream.Position + initialLenght;

            base.Deserialize(ref obj, stream);

            stream.BaseStream.Position = finalPosition;

            int lenght = stream.ReadInt32();
            Debug.Assert(lenght == initialLenght);
        }

        public override void Serialize(T obj, BinaryWriter stream)
        {
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

        public LenghtBoundSerializator(object additionalContext = null)
        {
            this.additionalContext = additionalContext;
        }
    }
}
