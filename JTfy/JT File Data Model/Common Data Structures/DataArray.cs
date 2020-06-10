using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JTfy
{
    public class DataArray<T> : BaseDataStructure
    {
        protected T[] data;

        public override int ByteCount
        {
            get
            {
                return data.Length * Marshal.SizeOf(typeof(T));
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                for (int i = 0, c = data.Length; i < c; ++i)
                {
                    bytesList.AddRange(StreamUtils.ToBytes(data[i]));
                }

                return bytesList.ToArray();
            }
        }
    }
}