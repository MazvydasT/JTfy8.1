using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class GUID : BaseDataStructure
    {
        private Guid guid;

        public static int Size { get { return 16; } }

        public override int ByteCount { get { return Size; } }

        public override byte[] Bytes
        {
            get
            {
                using (var guidStream = new MemoryStream(guid.ToByteArray()))
                {
                    var bytesList = new List<byte>(ByteCount);

                    bytesList.AddRange(StreamUtils.ReadBytes(guidStream, 4, true));
                    bytesList.AddRange(StreamUtils.ReadBytes(guidStream, 2, true));
                    bytesList.AddRange(StreamUtils.ReadBytes(guidStream, 2, true));
                    
                    bytesList.AddRange(StreamUtils.ReadBytes(guidStream, 8, false));

                    return bytesList.ToArray();
                }
            }
        }

        public GUID(Stream stream)
        {
            guid = new Guid(StreamUtils.ReadInt32(stream), StreamUtils.ReadInt16(stream), StreamUtils.ReadInt16(stream), StreamUtils.ReadBytes(stream, 8, false));
        }

        public GUID(string guidString)
        {
            guid = new Guid(guidString);
        }

        private GUID(Guid guid)
        {
            this.guid = guid;
        }

        public static GUID NewGUID()
        {
            return new GUID(Guid.NewGuid());
        }

        override public string ToString()
        {
            return guid.ToString("X");
        }
    }
}