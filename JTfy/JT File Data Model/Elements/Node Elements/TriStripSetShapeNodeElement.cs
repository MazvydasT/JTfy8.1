namespace JTfy
{
    public class TriStripSetShapeNodeElement : VertexShapeNodeElement
    {
        public override int ByteCount { get { return base.ByteCount; } }

        public override byte[] Bytes { get { return base.Bytes; } }


        public TriStripSetShapeNodeElement(GeometricSet geometrySet, int objectId) : base(geometrySet, objectId) { }

        public TriStripSetShapeNodeElement(Stream stream) : base(stream) { }
    }
}