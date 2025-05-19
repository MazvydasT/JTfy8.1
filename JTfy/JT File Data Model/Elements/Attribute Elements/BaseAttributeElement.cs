namespace JTfy
{
    public class BaseAttributeElement : BaseDataStructure
    {
        public int ObjectId { get; protected set; }
        public byte StateFlags { get; protected set; }
        public uint FieldInhibitFlags { get; protected set; }

        public override int ByteCount
        {
            get { return 4 + 1 + 4; }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(ObjectId));
                bytesList.Add(StateFlags);
                bytesList.AddRange(StreamUtils.ToBytes(FieldInhibitFlags));

                return bytesList.ToArray();
            }
        }

        public BaseAttributeElement(int objectId)
        {
            ObjectId = objectId;
            StateFlags = 0;
            FieldInhibitFlags = 0;
        }

        public BaseAttributeElement(Stream stream)
        {
            ObjectId = StreamUtils.ReadInt32(stream);
            StateFlags = StreamUtils.ReadByte(stream);
            FieldInhibitFlags = StreamUtils.ReadUInt32(stream);
        }
    }
}