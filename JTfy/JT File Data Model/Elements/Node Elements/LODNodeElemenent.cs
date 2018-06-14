using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class LODNodeElement : GroupNodeElement
    {
        private VecF32 reservedField1 = new VecF32();
        private int reservedField2 = 0;

        public override int ByteCount { get { return base.ByteCount + reservedField1.ByteCount + 4; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(reservedField1.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(reservedField2));

                return bytesList.ToArray();
            }
        }

        public LODNodeElement(int objectId, int[] childNodeObjectIds = null, int[] attributeObjectIds = null)
            : base(objectId, childNodeObjectIds, attributeObjectIds) { }

        public LODNodeElement(Stream stream)
            : base(stream)
        {
            reservedField1 = new VecF32(stream);
            reservedField2 = StreamUtils.ReadInt32(stream);
        }
    }
}