using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class QuantizationParameters : BaseDataStructure
    {
        public byte BitsPerVertex { get; protected set; }
        public byte NormalBitsFactor { get; protected set; }
        public byte BitsPerTextureCoord { get; protected set; }
        public byte BitsPerColor { get; protected set; }

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
                var bytesList = new List<byte>(ByteCount);

                bytesList.Add(BitsPerVertex);
                bytesList.Add(NormalBitsFactor);
                bytesList.Add(BitsPerTextureCoord);
                bytesList.Add(BitsPerColor);

                return bytesList.ToArray();
            }
        }

        public QuantizationParameters(Byte bitsPerVertex, Byte normalBitsFactor, Byte bitsPerTextureCoord, Byte bitsPerColor)
        {
            BitsPerVertex = bitsPerVertex;
            NormalBitsFactor = normalBitsFactor;
            BitsPerTextureCoord = bitsPerTextureCoord;
            BitsPerColor = bitsPerColor;
        }

        public QuantizationParameters(Stream stream) : this(StreamUtils.ReadByte(stream), StreamUtils.ReadByte(stream), StreamUtils.ReadByte(stream), StreamUtils.ReadByte(stream)) { }
    }
}