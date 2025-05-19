namespace JTfy
{
    public class InstanceNodeElement : BaseNodeElement
    {
        public int ChildNodeObjectID { get; private set; }

        public override int ByteCount
        {
            get
            {
                return base.ByteCount + 4;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(ChildNodeObjectID));

                return bytesList.ToArray();
            }
        }

        public InstanceNodeElement(int childNodeObjectID, int objectId)
            : base(objectId)
        {
            ChildNodeObjectID = childNodeObjectID;
        }

        public InstanceNodeElement(Stream stream)
            : base(stream)
        {
            ChildNodeObjectID = StreamUtils.ReadInt32(stream);
        }
    }
}