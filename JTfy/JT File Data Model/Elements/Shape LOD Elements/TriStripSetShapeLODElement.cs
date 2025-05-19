namespace JTfy
{
    public class TriStripSetShapeLODElement : VertexShapeLODElement
    {
        private readonly Int16 versionNumber = 1;
        public VertexBasedShapeCompressedRepData VertexBasedShapeCompressedRepData { get; private set; }

        public override int ByteCount
        {
            get { return base.ByteCount + 2 + VertexBasedShapeCompressedRepData.ByteCount; }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(versionNumber));
                bytesList.AddRange(VertexBasedShapeCompressedRepData.Bytes);

                return bytesList.ToArray();
            }
        }

        public TriStripSetShapeLODElement(int[][] triStrips, float[][] vertexPositions, float[][] vertexNormals = null) : this(new VertexBasedShapeCompressedRepData(triStrips, vertexPositions, vertexNormals)) { }

        public TriStripSetShapeLODElement(VertexBasedShapeCompressedRepData vertexBasedShapeCompressedRepData)
            : base(vertexBasedShapeCompressedRepData.NormalBinding, vertexBasedShapeCompressedRepData.QuantizationParameters)
        {
            VertexBasedShapeCompressedRepData = vertexBasedShapeCompressedRepData;
        }

        public TriStripSetShapeLODElement(Stream stream)
            : base(stream)
        {
            versionNumber = StreamUtils.ReadInt16(stream);
            VertexBasedShapeCompressedRepData = new VertexBasedShapeCompressedRepData(stream);
        }
    }
}