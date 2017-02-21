using System.IO;

namespace AmphetamineSerializer.Interfaces
{
    public interface ISerializer
    {
        void Serialize(object sample, BinaryWriter stream);
        void Deserialize(ref object sample, BinaryReader stream);
    }

    public interface ISerializer<T> : ISerializer
    {
        void Serialize(T sample, BinaryWriter stream);
        void Deserialize(ref T sample, BinaryReader stream);
    }
}