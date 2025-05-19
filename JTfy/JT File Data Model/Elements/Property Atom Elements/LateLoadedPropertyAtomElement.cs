namespace JTfy
{
    public class LateLoadedPropertyAtomElement : BasePropertyAtomElement
    {
        public GUID SegmentId { get; private set; }
        public int SegmentType { get; private set; }

        public override int ByteCount
        {
            get
            {
                return base.ByteCount + GUID.Size + 4;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesArray = new List<Byte>(ByteCount);

                bytesArray.AddRange(base.Bytes);
                bytesArray.AddRange(SegmentId.Bytes);
                bytesArray.AddRange(StreamUtils.ToBytes(SegmentType));

                return bytesArray.ToArray();
            }
        }

        public LateLoadedPropertyAtomElement(GUID segmentId, int segmentType, int objectId)
            : base(objectId)
        {
            SegmentId = segmentId;
            SegmentType = segmentType;
        }

        public LateLoadedPropertyAtomElement(Stream stream)
            : base(stream)
        {
            SegmentId = new GUID(stream);
            SegmentType = StreamUtils.ReadInt32(stream);
        }
    }
}