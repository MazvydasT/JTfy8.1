namespace JTfy
{
    public class BasePropertyAtomElement : BaseDataStructure
    {
        public int ObjectID { get; protected set; }
        public uint StateFlags { get; protected set; }

        public override int ByteCount
        {
            get
            {
                return 4 + 4;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(ObjectID));
                bytesList.AddRange(StreamUtils.ToBytes(StateFlags));

                return bytesList.ToArray();
            }
        }

        public BasePropertyAtomElement(int objectId)
        {
            ObjectID = objectId;
            StateFlags = 0;
        }

        public BasePropertyAtomElement(Stream stream)
        {
            ObjectID = StreamUtils.ReadInt32(stream);
            StateFlags = StreamUtils.ReadUInt32(stream);
        }
    }
}