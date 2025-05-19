namespace JTfy
{
    public class PartNodeElement : MetaDataNodeElement
    {
        private readonly Int16 versionNumber = 1;
        private readonly int reservedField = 0;

        public override int ByteCount { get { return base.ByteCount + 2 + 4; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(base.Bytes);

                bytesList.AddRange(StreamUtils.ToBytes(versionNumber));
                bytesList.AddRange(StreamUtils.ToBytes(reservedField));

                return bytesList.ToArray();
            }
        }

        public PartNodeElement(int objectId) : base(objectId) { }

        public PartNodeElement(Stream stream)
            : base(stream)
        {
            versionNumber = StreamUtils.ReadInt16(stream);
            reservedField = StreamUtils.ReadInt32(stream);
        }
    }
}