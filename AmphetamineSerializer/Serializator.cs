using AmphetamineSerializer.Backends;
using AmphetamineSerializer.Common.Chain;
using AmphetamineSerializer.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace AmphetamineSerializer
{
    /// <summary>
    /// AmphetamineSerializer's default serializator.
    /// </summary>
    /// <typeparam name="T">Type of the object to serialize/deserialize</typeparam>
    public class Serializator<T> : ISerializator<T>
    {
        protected delegate void SerializeStream(T obj, BinaryWriter stream);
        protected delegate void DeserializeStream(ref T obj, BinaryReader stream);
        protected delegate void SerializeByteArray(T obj, byte[] array, ref int position);
        protected delegate void DeserializeByteArray(ref T obj, byte[] array, ref int position);

        protected Lazy<DeserializeStream> DeserializeFromStream { get; }
        protected Lazy<SerializeStream> SerializeFromStream { get; }
        protected Lazy<DeserializeByteArray> DeserializeFromByteArray { get; }
        protected Lazy<SerializeByteArray> SerializeFromByteArray { get; }

        private Dictionary<string, object> additionalContext;
        DefaultHandlerFinder finder = DefaultHandlerFinder.StreamTemplate();

        /// <summary>
        /// Build the serializator.
        /// </summary>
        /// <param name="additionalContext">Additional context that may serve to some backend.</param>
        public Serializator(Dictionary<string, object> additionalContext= null)
        {
            this.additionalContext = additionalContext;

            DeserializeFromByteArray = new Lazy<DeserializeByteArray>(() => Build<DeserializeByteArray>());
            SerializeFromByteArray = new Lazy<SerializeByteArray>(() => Build<SerializeByteArray>());
            DeserializeFromStream = new Lazy<DeserializeStream>(() => Build<DeserializeStream>());
            SerializeFromStream = new Lazy<SerializeStream>(() => Build<SerializeStream>());
        }

        public virtual void Deserialize(ref T obj, BinaryReader stream)
        {
            DeserializeFromStream.Value(ref obj, stream);
        }

        public virtual void Serialize(T obj, BinaryWriter stream)
        {
            SerializeFromStream.Value(obj, stream);
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

        private D Build<D>()
        {
            var request = new DelegateBuildRequest()
            {
                AdditionalContext = additionalContext,
                DelegateType = typeof(D)
            };

            var response = finder.RequestHandler(request) as DelegateBuildResponse;
            return (D)Convert.ChangeType(response.Delegate, typeof(D));
        }

        public void Serialize(T obj, byte[] array, ref int position)
        {
            SerializeFromByteArray.Value(obj, array, ref position);
        }

        public void Deserialize(ref T obj, byte[] array, ref int position)
        {
            DeserializeFromByteArray.Value(ref obj, array, ref position);
        }

        public void Serialize(object obj, byte[] array, ref int position)
        {
            SerializeFromByteArray.Value((T)obj, array, ref position);
        }

        public void Deserialize(ref object obj, byte[] array, ref int position)
        {
            var request = (T)obj;
            DeserializeFromByteArray.Value(ref request, array, ref position);
            obj = request;
        }
    }
}
