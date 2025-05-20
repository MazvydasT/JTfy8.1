namespace JTfy
{
    public class UniformQuantizerData(Stream stream) : BaseDataStructure
    {
        public Single Min { get; protected set; } = StreamUtils.ReadFloat(stream);
        public Single Max { get; protected set; } = StreamUtils.ReadFloat(stream);
        public Byte NumberOfBits { get; protected set; } = StreamUtils.ReadByte(stream);

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

                return [.. bytesList];
            }
        }
    }
}