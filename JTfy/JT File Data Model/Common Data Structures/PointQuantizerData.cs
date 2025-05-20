namespace JTfy
{
    public class PointQuantizerData(UniformQuantizerData x, UniformQuantizerData y, UniformQuantizerData z) : BaseDataStructure
    {
        public UniformQuantizerData X { get; private set; } = x;
        public UniformQuantizerData Y { get; private set; } = y;
        public UniformQuantizerData Z { get; private set; } = z;

        public override int ByteCount { get { return X.ByteCount + Y.ByteCount + Z.ByteCount; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(X.Bytes);
                bytesList.AddRange(Y.Bytes);
                bytesList.AddRange(Z.Bytes);

                return [.. bytesList];
            }
        }

        public PointQuantizerData(Stream stream) : this(new UniformQuantizerData(stream), new UniformQuantizerData(stream), new UniformQuantizerData(stream)) { }
    }
}