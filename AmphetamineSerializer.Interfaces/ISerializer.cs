using System.IO;

namespace AmphetamineSerializer.Interfaces
{
    public interface ISerializer
    {
        void Serialize(ref object sample, BinaryWriter stream);
        void Deserialize(ref object sample, BinaryReader stream);
    }

    public interface ISerializer<T> : ISerializer
    {
        void Serialize(ref T sample, BinaryWriter stream);
        void Deserialize(ref T sample, BinaryReader stream);
    }
}