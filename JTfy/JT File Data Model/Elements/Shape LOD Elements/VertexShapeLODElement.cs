namespace JTfy
{
    public class VertexShapeLODElement : BaseDataStructure
    {
        private readonly Int16 versionNumber = 1;
        public int BindingAttributes { get; private set; }
        public QuantizationParameters QuantizationParameters { get; private set; }

        public override int ByteCount
        {
            get { return 2 + 4 + QuantizationParameters.ByteCount; }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(versionNumber));
                bytesList.AddRange(StreamUtils.ToBytes(BindingAttributes));
                bytesList.AddRange(QuantizationParameters.Bytes);

                return bytesList.ToArray();
            }
        }

        public VertexShapeLODElement(int bindingAttributes, QuantizationParameters quantizationParameters)
        {
            BindingAttributes = bindingAttributes;
            QuantizationParameters = quantizationParameters;
        }

        public VertexShapeLODElement(Stream stream)
        {
            versionNumber = StreamUtils.ReadInt16(stream);

            BindingAttributes = StreamUtils.ReadInt32(stream);
            QuantizationParameters = new QuantizationParameters(stream);
        }
    }
}