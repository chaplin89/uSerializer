using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
