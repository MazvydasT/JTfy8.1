using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class RangeLODNodeElement : LODNodeElement
    {
        private VecF32 rangeLimits = new VecF32();
        public VecF32 RangeLimits { get { return rangeLimits; } set { rangeLimits = value ?? new VecF32(); } }

        private CoordF32 center = new CoordF32();
        public CoordF32 Center { get { return center; } set { center = value ?? new CoordF32(); } }

        public override int ByteCount { get { return base.ByteCount + RangeLimits.ByteCount + Center.ByteCount; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(RangeLimits.Bytes);
                bytesList.AddRange(Center.Bytes);

                return bytesList.ToArray();
            }
        }

        public RangeLODNodeElement(int objectId) : base(objectId) { }

        public RangeLODNodeElement(Stream stream) : base(stream)
        {
            RangeLimits = new VecF32(stream);
            Center = new CoordF32(stream);
        }
    }
}