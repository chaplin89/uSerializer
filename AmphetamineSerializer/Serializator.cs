using AmphetamineSerializer.Chain;
using AmphetamineSerializer.Chain.Nodes;
using AmphetamineSerializer.Interfaces;
using System;
using System.IO;

namespace AmphetamineSerializer
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Serializator<T> : ISerializator
    {
        protected delegate void DeserializeBytes(ref T obj, byte[] buffer, ref uint position);

        protected delegate void SerializeBinaryWriter(T obj, BinaryWriter stream);

        protected delegate void DeserializeBinaryReader(ref T obj, BinaryReader stream);

        protected DeserializeBytes deserializeFromBytes;
        protected DeserializeBinaryReader deserializeFromStream;
        protected SerializeBinaryWriter serializeFromStream;

        private object additionalContext;
        IChainManager chain = new ChainManager()
                                  // .SetNext(new CustomSerializerFinder())
                                  // .SetNext(new CustomBuilderFinder())
                                  .SetNext(new DefaultHandlerFinder())
                                  .SetNext(new CacheManager());

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="buffer"></param>
        /// <param name="position"></param>
        public virtual void Deserialize(ref T obj, byte[] buffer, ref uint position)
        {
            if (deserializeFromBytes == null)
                deserializeFromBytes = (DeserializeBytes)MakeRequest(typeof(DeserializeBytes));

            deserializeFromBytes(ref obj, buffer, ref position);
        }

        public virtual void Deserialize(ref T obj, BinaryReader stream)
        {
            if (deserializeFromStream == null)
                deserializeFromStream = (DeserializeBinaryReader)MakeRequest(typeof(DeserializeBinaryReader));

            deserializeFromStream(ref obj, stream);
        }

        public virtual void Serialize(T obj, BinaryWriter stream)
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
                AdditionalContext = additionalContext,
                DelegateType = delegateType,
                RequestType = TypeOfRequest.Delegate
            };

            var response = chain.Process(request) as SerializationBuildResponse;
            return response.Response.Delegate;
        }

        public Serializator(object additionalContext = null)
        {
            this.additionalContext = additionalContext;
        }
    }
}
