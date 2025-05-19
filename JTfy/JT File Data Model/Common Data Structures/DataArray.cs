using System.Runtime.InteropServices;

namespace JTfy
{
    public class DataArray<T> : BaseDataStructure
    {
        protected T[] data;

        public override int ByteCount => data.Length * Marshal.SizeOf<T>();

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                for (int i = 0, c = data.Length; i < c; ++i)
                {
                    bytesList.AddRange(StreamUtils.ToBytes(data[i]));
                }

                return [.. bytesList];
            }
        }
    }
}