﻿using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class LosslessCompressedRawVertexData : BaseDataStructure
    {
        public int UncompressedDataSize { get; private set; }
        public int CompressedDataSize { get; private set; }

        private byte[] compressedVertexData;
        private byte[] vertexData;

        public byte[] CompressedVertexData
        {
            get
            {
                if (compressedVertexData == null)
                {
                    compressedVertexData = CompressionUtils.Compress(vertexData);
                }

                return compressedVertexData;
            }

            set
            {
                compressedVertexData = value == null ? new byte[0] : value;
            }
        }

        public byte[] VertexData
        {
            get
            {
                if (vertexData == null)
                {
                    vertexData = CompressionUtils.Decompress(compressedVertexData);
                }

                return vertexData;
            }

            set
            {
                vertexData = value == null ? new byte[0] : value;
            }
        }

        public override int ByteCount
        {
            get { return 4 + 4 + CompressedVertexData.Length; }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(VertexData.Length));
                bytesList.AddRange(StreamUtils.ToBytes(CompressedVertexData.Length));
                bytesList.AddRange(CompressedVertexData);

                return bytesList.ToArray();
            }
        }

        public LosslessCompressedRawVertexData(byte[] vertexData)
        {
            VertexData = vertexData;

            UncompressedDataSize = vertexData.Length;
            CompressedDataSize = CompressedVertexData.Length;
        }

        public LosslessCompressedRawVertexData(Stream stream)
        {
            UncompressedDataSize = StreamUtils.ReadInt32(stream);
            CompressedDataSize = StreamUtils.ReadInt32(stream);

            if (CompressedDataSize > 0)
                CompressedVertexData = StreamUtils.ReadBytes(stream, CompressedDataSize, false);
            else if (CompressedDataSize < 0)
                VertexData = StreamUtils.ReadBytes(stream, Math.Abs(CompressedDataSize), false);
            else
                VertexData = CompressedVertexData = new byte[0];
        }
    }
}