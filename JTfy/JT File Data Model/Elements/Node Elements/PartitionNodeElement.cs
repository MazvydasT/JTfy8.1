using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class PartitionNodeElement : GroupNodeElement
    {
        public int PartitionFlags { get; private set; }

        private MbString fileName = new MbString();
        public MbString FileName { get { return fileName; } set { fileName = value; } }

        private BBoxF32 transformedBBox = new BBoxF32();
        public BBoxF32 TransformedBBox { get { return transformedBBox; } set { transformedBBox = value; } }

        public float Area { get; set; }

        private CountRange vertexCountRange = new CountRange();
        public CountRange VertexCountRange { get { return vertexCountRange; } set { vertexCountRange = value; } }

        private CountRange nodeCountRange = new CountRange();
        public CountRange NodeCountRange { get { return nodeCountRange; } set { nodeCountRange = value; } }

        private CountRange polygonCountRange = new CountRange();
        public CountRange PolygonCountRange { get { return polygonCountRange; } set { polygonCountRange = value; } }

        private BBoxF32 untransformedBBox = null;
        public BBoxF32 UntransformedBBox { get { return untransformedBBox; } set { untransformedBBox = value; PartitionFlags = value == null ? 0 : 1; } }

        override public Int32 ByteCount
        {
            get
            {
                return base.ByteCount + 4 + FileName.ByteCount + (TransformedBBox == null ? 0 : TransformedBBox.ByteCount) + 4 + 3 * VertexCountRange.ByteCount + (UntransformedBBox == null ? 0 : UntransformedBBox.ByteCount);
            }
        }

        override public Byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(PartitionFlags));
                bytesList.AddRange(FileName.Bytes);

                if (TransformedBBox != null)
                {
                    bytesList.AddRange(TransformedBBox.Bytes);
                }

                bytesList.AddRange(StreamUtils.ToBytes(Area));
                bytesList.AddRange(VertexCountRange.Bytes);
                bytesList.AddRange(NodeCountRange.Bytes);
                bytesList.AddRange(PolygonCountRange.Bytes);

                if (UntransformedBBox != null)
                {
                    bytesList.AddRange(UntransformedBBox.Bytes);
                }

                return bytesList.ToArray();
            }
        }

        public PartitionNodeElement(int objectId, PartitionNodeElement element)
            : this(objectId)
        {
            PartitionFlags = element.PartitionFlags;
            FileName = element.FileName;
            TransformedBBox = element.TransformedBBox;
            Area = element.Area;
            VertexCountRange = element.VertexCountRange;
            NodeCountRange = element.NodeCountRange;
            PolygonCountRange = element.PolygonCountRange;
            UntransformedBBox = element.UntransformedBBox;
        }

        public PartitionNodeElement(int objectId) : base(objectId) { }

        public PartitionNodeElement(Stream stream)
            : base(stream)
        {
            PartitionFlags = StreamUtils.ReadInt32(stream);
            FileName = new MbString(stream);
            TransformedBBox = new BBoxF32(stream);
            Area = StreamUtils.ReadFloat(stream);
            VertexCountRange = new CountRange(stream);
            NodeCountRange = new CountRange(stream);
            PolygonCountRange = new CountRange(stream);

            if ((PartitionFlags & 0x00000001) != 0)
            {
                UntransformedBBox = new BBoxF32(stream);
            }
        }
    }
}