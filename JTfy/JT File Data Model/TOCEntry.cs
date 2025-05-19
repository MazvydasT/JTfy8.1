namespace JTfy
{
    public class TOCEntry : BaseDataStructure
    {
        public GUID SegmentID { get; private set; }
        public int SegmentOffset { get; set; }
        public int SegmentLength { get; private set; }
        public uint SegmentAttributes { get; private set; }

        public static int Size { get { return GUID.Size + 4 + 4 + 4; } }

        public override int ByteCount { get { return Size; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(SegmentID.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(SegmentOffset));
                bytesList.AddRange(StreamUtils.ToBytes(SegmentLength));
                bytesList.AddRange(StreamUtils.ToBytes(SegmentAttributes));

                return bytesList.ToArray();
            }
        }

        public TOCEntry(GUID segmentID, int segmentOffset, int segmentLength, uint segmentAttributes)
        {
            SegmentID = segmentID;
            SegmentOffset = segmentOffset;
            SegmentLength = segmentLength;
            SegmentAttributes = segmentAttributes;
        }

        public TOCEntry(Stream stream) : this(new GUID(stream), StreamUtils.ReadInt32(stream), StreamUtils.ReadInt32(stream), StreamUtils.ReadUInt32(stream)) { }
    }
}