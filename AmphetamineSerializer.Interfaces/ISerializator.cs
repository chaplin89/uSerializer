using System.IO;

namespace AmphetamineSerializer.Interfaces
{
    public interface ISerializator<T> : ISerializator
    {
        void Serialize(T obj, BinaryWriter stream);
        void Deserialize(ref T obj, BinaryReader stream);

        void Serialize(T obj, byte[] array, ref int position);
        void Deserialize(ref T obj, byte[] array, ref int position);
    }

    public interface ISerializator
    {
        void Serialize(object obj, BinaryWriter stream);
        void Deserialize(ref object obj, BinaryReader stream);

        void Serialize(object obj, byte[] array, ref int position);
        void Deserialize(ref object obj, byte[] array, ref int position);
    }
}
