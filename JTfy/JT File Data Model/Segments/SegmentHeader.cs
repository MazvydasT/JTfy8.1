namespace JTfy
{
    public class SegmentHeader(GUID segmentID, int segmentType, int segmentLength) : BaseDataStructure
    {
        public GUID SegmentID { get; private set; } = segmentID;
        public int SegmentType { get; private set; } = segmentType;
        public int SegmentLength { get; private set; } = segmentLength;

        public static int Size { get { return GUID.Size + 4 + 4; } }

        public override int ByteCount { get { return Size; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(SegmentID.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(SegmentType));
                bytesList.AddRange(StreamUtils.ToBytes(SegmentLength));

                return [.. bytesList];
            }
        }

        public override string ToString()
        {
            return String.Format("{0}|{1}|{2}", SegmentID, SegmentType, SegmentLength);
        }

        public SegmentHeader(Stream stream) : this(new GUID(stream), StreamUtils.ReadInt32(stream), StreamUtils.ReadInt32(stream)) { }
    }
}