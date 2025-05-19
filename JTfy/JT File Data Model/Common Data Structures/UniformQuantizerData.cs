namespace JTfy
{
    public class UniformQuantizerData : BaseDataStructure
    {
        public Single Min { get; protected set; }
        public Single Max { get; protected set; }
        public Byte NumberOfBits { get; protected set; }

        public override int ByteCount
        {
            get
            {
                return 4 + 4 + 1;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(Min));
                bytesList.AddRange(StreamUtils.ToBytes(Max));
                bytesList.Add(NumberOfBits);

                return bytesList.ToArray();
            }
        }

        public UniformQuantizerData(Stream stream)
        {
            Min = StreamUtils.ReadFloat(stream);
            Max = StreamUtils.ReadFloat(stream);
            NumberOfBits = StreamUtils.ReadByte(stream);
        }
    }
}