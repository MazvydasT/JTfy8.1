namespace JTfy
{
    public class PointQuantizerData : BaseDataStructure
    {
        public UniformQuantizerData X { get; private set; }
        public UniformQuantizerData Y { get; private set; }
        public UniformQuantizerData Z { get; private set; }

        public override int ByteCount { get { return X.ByteCount + Y.ByteCount + Z.ByteCount; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(X.Bytes);
                bytesList.AddRange(Y.Bytes);
                bytesList.AddRange(Z.Bytes);

                return bytesList.ToArray();
            }
        }

        public PointQuantizerData(UniformQuantizerData x, UniformQuantizerData y, UniformQuantizerData z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public PointQuantizerData(Stream stream) : this(new UniformQuantizerData(stream), new UniformQuantizerData(stream), new UniformQuantizerData(stream)) { }
    }
}