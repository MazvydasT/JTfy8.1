namespace JTfy
{
    public class Vec<T> : DataArray<T>
    {
        public Int32 Count { get { return data.Length; } }

        public override Int32 ByteCount { get { return 4 + base.ByteCount; } }

        public override Byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(Count));

                for (int i = 0; i < Count; ++i)
                {
                    bytesList.AddRange(StreamUtils.ToBytes(data[i]));
                }

                return bytesList.ToArray();
            }
        }

        public Vec(Stream stream)
        {
            data = new T[StreamUtils.ReadInt32(stream)];

            for (int i = 0, c = data.Length; i < c; ++i)
            {
                data[i] = StreamUtils.Read<T>(stream);
            }
        }

        public Vec(T[] data)
        {
            this.data = data;
        }
    }

    public class VecI32 : Vec<Int32>
    {
        public VecI32(Stream stream) : base(stream) { }
        public VecI32(int[] data) : base(data) { }
        public VecI32() : this(new int[0]) { }
    }

    public class VecU32 : Vec<UInt32>
    {
        public VecU32(Stream stream) : base(stream) { }
        public VecU32(uint[] data) : base(data) { }
        public VecU32() : this(new uint[0]) { }
    }

    public class VecF32 : Vec<Single>
    {
        public VecF32(Stream stream) : base(stream) { }
        public VecF32(float[] data) : base(data) { }
        public VecF32() : this(new float[0]) { }
    }

    public class MbString : Vec<UInt16>
    {
        public string Value
        {
            get
            {
                var chars = new char[data.Length];
                Buffer.BlockCopy(data, 0, chars, 0, chars.Length * 2);

                return new String(chars);
            }
        }

        public override string ToString()
        {
            return Value;
        }

        public MbString(Stream stream) : base(stream) { }
        public MbString(UInt16[] data) : base(data) { }
        public MbString() : base(new ushort[0]) { }
        public MbString(string value)
            : base(new UInt16[0])
        {
            var chars = value.ToCharArray();

            data = new UInt16[chars.Length];

            Buffer.BlockCopy(chars, 0, data, 0, data.Length * 2);
        }
    }
}