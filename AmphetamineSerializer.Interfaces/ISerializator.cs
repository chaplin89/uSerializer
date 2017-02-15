using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmphetamineSerializer.Interfaces
{
    public interface ISerializator
    {
        void Deserialize(ref object obj, byte[] buffer, ref uint position);
        void Serialize(object obj, BinaryWriter stream);
        void Deserialize(ref object obj, BinaryReader stream);
    }
}
