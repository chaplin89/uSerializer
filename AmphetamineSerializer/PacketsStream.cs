using System;
using System.Collections.Generic;
using System.IO;

namespace AmphetamineSerializer
{
    /// <summary>
    /// 
    /// </summary>
    public class PacketsStream : IDisposable
    {
        Stream stream;
        byte[] fourByteArray = new byte[4];

        public PacketsStream(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] GetNext()
        {
            int read = stream.Read(fourByteArray, 0, 4);
            if (read != 4)
                return null;

            int size = BitConverter.ToInt32(fourByteArray, 0);

            byte[] buffer = new byte[size];
            read = stream.Read(buffer, 0, size);
            if (read != size)
                throw new Exception("Unexpected end of file");

            stream.Seek(4, SeekOrigin.Current);
            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void MoveToEnd()
        {
            stream.Seek(0, SeekOrigin.End);
        }

         /// <summary>
        /// 
        /// </summary>
        /// <param name="fixedSize"></param>
        /// <returns></returns>
        public IEnumerable<byte[]> GetPackets(uint fixedSize)
        {
            while (true)
            {
                byte[] buffer = new byte[fixedSize];
                int read = stream.Read(buffer, 0, (int)fixedSize);
                if (read == 0)
                    yield break;
                if (read != fixedSize)
                    throw new Exception("Unexpected end of file");
                yield return buffer;
            }
        }

        /// <summary>
        /// This function divide the stream in packets and assume this logical subdivision:
        /// 1. Record size
        /// 2. Packet
        /// 3. Record size
        /// </summary>
        /// <returns></returns>
        public IEnumerable<byte[]> GetPackets()
        {
            while (true)
            {
                int size;
                int read = stream.Read(fourByteArray, 0, 4);
                if (read != 4)
                    yield break;
                size = BitConverter.ToInt32(fourByteArray, 0);
                byte[] buffer = new byte[size];
                read = stream.Read(buffer, 0, size);
                if (read != size)
                    throw new Exception("Unexpected end of file");

                stream.Seek(4, SeekOrigin.Current);
                yield return buffer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<byte[]> GetPacketsBackward()
        {
            while (true)
            {
                stream.Seek(-4, SeekOrigin.Current);
                int read = stream.Read(fourByteArray, 0, 4);

                if (read != 4)
                    yield break;

                int size = BitConverter.ToInt32(fourByteArray, 0);

                if (stream.Position - size == 8)
                    yield break;

                byte[] buffer = new byte[size];

                stream.Seek(-size - 4, SeekOrigin.Current);
                read = stream.Read(buffer, 0, size);
                if (read != size)
                    throw new Exception("Unexpected end of file");
                stream.Seek(-size - 4, SeekOrigin.Current);
                yield return buffer;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Stream CurrentStream { get { return stream; } }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool unused)
        {
            stream.Dispose();
        }
    }
}
