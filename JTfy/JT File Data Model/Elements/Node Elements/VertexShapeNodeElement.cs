using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class VertexShapeNodeElement : BaseShapeNodeElement
    {
        public int NormalBinding { get; protected set; }
        public int TextureBinding { get; protected set; }
        public int ColourBinding { get; protected set; }
        public QuantizationParameters QuantizationParameters { get; protected set; }

        public override int ByteCount
        {
            get { return base.ByteCount + 4 + 4 + 4 + QuantizationParameters.ByteCount; }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(NormalBinding));
                bytesList.AddRange(StreamUtils.ToBytes(TextureBinding));
                bytesList.AddRange(StreamUtils.ToBytes(ColourBinding));
                bytesList.AddRange(QuantizationParameters.Bytes);

                return bytesList.ToArray();
            }
        }

        public VertexShapeNodeElement(Stream stream)
            : base(stream)
        {
            NormalBinding = StreamUtils.ReadInt32(stream);
            TextureBinding = StreamUtils.ReadInt32(stream);
            ColourBinding = StreamUtils.ReadInt32(stream);
            QuantizationParameters = new QuantizationParameters(stream);
        }

        public VertexShapeNodeElement(GeometricSet geometricSet, int objectId)
            : base(geometricSet, objectId)
        {
            NormalBinding = geometricSet.Normals == null ? 1 : 0;
            TextureBinding = 0;
            ColourBinding = 0;
            QuantizationParameters = new QuantizationParameters(0, 0, 0, 0);
        }
    }
}