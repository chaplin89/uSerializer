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
    public class Serializator<T> : ISerializator<T>
    {
        protected delegate void SerializeBinaryWriter(T obj, BinaryWriter stream);
        protected delegate void DeserializeBinaryReader(ref T obj, BinaryReader stream);
        
        protected DeserializeBinaryReader deserializeFromStream;
        protected SerializeBinaryWriter serializeFromStream;

        private object additionalContext;
        IChainManager chain = new ChainManager()
                                  //.SetNext(new CustomSerializerFinder())
                                  //.SetNext(new CustomBuilderFinder())
                                  .SetNext(new DefaultHandlerFinder())
                                  .SetNext(new CacheManager());
        
        public Serializator(object additionalContext = null)
        {
            this.additionalContext = additionalContext;
        }

        public virtual void Deserialize(ref T obj, BinaryReader stream)
        {
            if (deserializeFromStream == null)
                deserializeFromStream = (DeserializeBinaryReader)Build(typeof(DeserializeBinaryReader));

            deserializeFromStream(ref obj, stream);
        }

        public virtual void Serialize(T obj, BinaryWriter stream)
        {
            if (serializeFromStream == null)
                serializeFromStream = (SerializeBinaryWriter)Build(typeof(SerializeBinaryWriter));

            serializeFromStream(obj, stream);
        }

        public void Serialize(object obj, BinaryWriter stream)
        {
            Serialize((T)obj, stream);
        }

        public void Deserialize(ref object obj, BinaryReader stream)
        {
            T request = (T)obj;
            Deserialize(ref request, stream);
            obj = request;
        }

        private Delegate Build(Type delegateType)
        {
            var request = new SerializationBuildRequest()
            {
                AdditionalContext = additionalContext,
                DelegateType = delegateType,
                RequestType = TypeOfRequest.Delegate
            };

            var response = chain.Process(request) as SerializationBuildResponse;
            return response.Function.Delegate;
        }
    }
}
