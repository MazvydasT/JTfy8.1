namespace JTfy
{
    public class MetaDataNodeElement : GroupNodeElement
    {
        private readonly Int16 versionNumber = 1;

        public override int ByteCount { get { return base.ByteCount + 2; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(versionNumber));

                return bytesList.ToArray();
            }
        }

        public MetaDataNodeElement(int objectId) : base(objectId) { }

        public MetaDataNodeElement(Stream stream) : base(stream)
        {
            versionNumber = StreamUtils.ReadInt16(stream);
        }
    }
}