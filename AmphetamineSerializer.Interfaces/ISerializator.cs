using System.IO;

namespace AmphetamineSerializer.Interfaces
{
    public interface ISerializator<T> : ISerializator
    {
        void Serialize(T obj, BinaryWriter stream);
        void Deserialize(ref T obj, BinaryReader stream);
    }

    public interface ISerializator
    {
        void Serialize(object obj, BinaryWriter stream);
        void Deserialize(ref object obj, BinaryReader stream);
    }
}
