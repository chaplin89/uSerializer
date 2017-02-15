using AmphetamineSerializer.Common;
using AmphetamineSerializer.Interfaces;
using System;
using System.IO;

namespace AmphetamineSerializer
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TWSerializator<T> : ISerializator
    {
        delegate void DeserializeBytes(ref T obj, byte[] buffer, ref uint position);
        delegate void SerializeBinaryWriter(T obj, BinaryWriter stream);
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
        public void Deserialize(ref T obj, byte[] buffer, ref uint position)
        {
            if (deserializeFromBytes == null)
                deserializeFromBytes = (DeserializeBytes)MakeRequest(typeof(DeserializeBytes));

            deserializeFromBytes(ref obj, buffer, ref position);
        }

        public void Deserialize(ref T obj, BinaryReader stream)
        {
            if (deserializeFromStream == null)
                deserializeFromStream = (DeserializeBinaryReader)MakeRequest(typeof(DeserializeBinaryReader));

            if (stream.BaseStream.Position == stream.BaseStream.Length)
            {
                obj = default(T);
                return;
            }

            deserializeFromStream(ref obj, stream);
        }

        public void Serialize(T obj, BinaryWriter stream)
        {
            if (serializeFromStream == null)
                serializeFromStream = (SerializeBinaryWriter)MakeRequest(typeof(SerializeBinaryWriter));
            serializeFromStream(obj, stream);
        }

        public void Deserialize(ref object obj, byte[] buffer, ref uint position)
        {
            T tempObj = (T)obj;
            Deserialize(ref obj, buffer, ref position);
            obj = tempObj;
        }

        public void Serialize(object obj, BinaryWriter stream)
        {
            Serialize((T)obj, stream);
        }

        public void Deserialize(ref object obj, BinaryReader stream)
        {
            T tempObj = (T)obj;
            Deserialize(ref tempObj, stream);
            obj = tempObj;
        }

        private Delegate MakeRequest(Type delegateType)
        {
            var request = new SerializationBuildRequest()
            {
                AdditionalContext = this.additionalContext,
                DelegateType = delegateType
            };
            var response = chain.Process(request) as SerializationBuildResponse;
            return response.Method.CreateDelegate(delegateType, response.Instance);
        }

        public TWSerializator(object additionalContext = null)
        {
            this.additionalContext = additionalContext;
        }
    }
}
