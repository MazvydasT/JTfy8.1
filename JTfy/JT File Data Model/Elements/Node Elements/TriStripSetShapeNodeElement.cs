using System;
using System.IO;

namespace JTfy
{
    public class TriStripSetShapeNodeElement : VertexShapeNodeElement
    {
        public override int ByteCount { get { return base.ByteCount; } }

        public override byte[] Bytes { get { return base.Bytes; } }


        public TriStripSetShapeNodeElement(GeometricSet geometrySet, int objectId, int[] attributeObjectIds = null) : base(geometrySet, objectId, attributeObjectIds) { }

        public TriStripSetShapeNodeElement(Stream stream) : base(stream) { }
    }
}