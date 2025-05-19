namespace JTfy
{
    public class BaseNodeElement : BaseDataStructure
    {
        public int ObjectId { get; protected set; }
        public uint NodeFlags { get; protected set; }

        public int AttributeCount { get { return AttributeObjectIds.Count; } }

        private List<int> attributeObjectIds = new List<int>();
        public List<int> AttributeObjectIds { get { return attributeObjectIds; } set { attributeObjectIds = value ?? new List<int>(); } }

        public override int ByteCount { get { return 4 + 4 + 4 + AttributeCount * 4; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(ObjectId));
                bytesList.AddRange(StreamUtils.ToBytes(NodeFlags));
                bytesList.AddRange(StreamUtils.ToBytes(AttributeCount));

                for (int i = 0; i < AttributeCount; ++i)
                {
                    bytesList.AddRange(StreamUtils.ToBytes(AttributeObjectIds[i]));
                }

                return bytesList.ToArray();
            }
        }

        public BaseNodeElement(int objectId)
        {
            ObjectId = objectId;
            NodeFlags = 0;
        }

        public BaseNodeElement(Stream stream)
        {
            ObjectId = StreamUtils.ReadInt32(stream);
            NodeFlags = StreamUtils.ReadUInt32(stream);

            var attributeCount = StreamUtils.ReadInt32(stream);
            AttributeObjectIds = new List<int>(attributeCount);

            for (int i = 0; i < attributeCount; ++i)
            {
                AttributeObjectIds.Add(StreamUtils.ReadInt32(stream));
            }
        }
    }
}