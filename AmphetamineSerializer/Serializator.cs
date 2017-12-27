using AmphetamineSerializer.Chain.Nodes;
using AmphetamineSerializer.Common.Chain;
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
        
        protected DeserializeBinaryReader DeserializeFromStream { get; set; }
        protected SerializeBinaryWriter SerializeFromStream { get; set; }

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
            if (DeserializeFromStream == null)
                DeserializeFromStream = (DeserializeBinaryReader)Build(typeof(DeserializeBinaryReader));

            DeserializeFromStream(ref obj, stream);
        }

        public virtual void Serialize(T obj, BinaryWriter stream)
        {
            if (SerializeFromStream == null)
                SerializeFromStream = (SerializeBinaryWriter)Build(typeof(SerializeBinaryWriter));

            SerializeFromStream(obj, stream);
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
            var request = new DelegateBuildRequest()
            {
                AdditionalContext = additionalContext,
                DelegateType = delegateType
            };

            var response = chain.Process(request) as DelegateBuildResponse;
            return response.Delegate;
        }
    }
}
