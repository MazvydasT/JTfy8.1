namespace JTfy
{
    public class QuantizationParameters(Byte bitsPerVertex, Byte normalBitsFactor, Byte bitsPerTextureCoord, Byte bitsPerColor) : BaseDataStructure
    {
        public byte BitsPerVertex { get; protected set; } = bitsPerVertex;
        public byte NormalBitsFactor { get; protected set; } = normalBitsFactor;
        public byte BitsPerTextureCoord { get; protected set; } = bitsPerTextureCoord;
        public byte BitsPerColor { get; protected set; } = bitsPerColor;

        public override int ByteCount
        {
            get
            {
                return 1 + 1 + 1 + 1;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount)
                {
                    BitsPerVertex,
                    NormalBitsFactor,
                    BitsPerTextureCoord,
                    BitsPerColor
                };

                return [.. bytesList];
            }
        }

        public QuantizationParameters(Stream stream) : this(StreamUtils.ReadByte(stream), StreamUtils.ReadByte(stream), StreamUtils.ReadByte(stream), StreamUtils.ReadByte(stream)) { }
    }
}