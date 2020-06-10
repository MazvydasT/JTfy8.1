﻿using System;
using System.Collections.Generic;
using System.IO;

using System.Diagnostics;

namespace JTfy
{
    public class LosslessCompressedRawVertexData : BaseDataStructure
    {
        public int UncompressedDataSize { get { return VertexData.Length; } }
        public int CompressedDataSize { get { return CompressedVertexData.Length; } }

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
                compressedVertexData = value ?? (new byte[0]);
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
                vertexData = value ?? (new byte[0]);
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
        }

        public LosslessCompressedRawVertexData(Stream stream)
        {
            var uncompressedDataSize = StreamUtils.ReadInt32(stream);
            var compressedDataSize = StreamUtils.ReadInt32(stream);

            if (compressedDataSize > 0)
                CompressedVertexData = StreamUtils.ReadBytes(stream, compressedDataSize, false);
            else if (compressedDataSize < 0)
                VertexData = StreamUtils.ReadBytes(stream, Math.Abs(compressedDataSize), false);
            else
                VertexData = CompressedVertexData = new byte[0];
        }
    }
}