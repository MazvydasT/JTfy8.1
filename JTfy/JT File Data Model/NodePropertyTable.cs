namespace JTfy
{
    public class NodePropertyTable : BaseDataStructure
    {
        public List<int> KeyPropertyAtomObjectIDs { get; protected set; }
        public List<int> ValuePropertyAtomObjectIDs { get; protected set; }

        public override int ByteCount
        {
            get
            {
                return (2 * KeyPropertyAtomObjectIDs.Count + 1) * 4;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                for (int i = 0, c = KeyPropertyAtomObjectIDs.Count; i < c; ++i)
                {
                    bytesList.AddRange(StreamUtils.ToBytes(KeyPropertyAtomObjectIDs[i]));
                    bytesList.AddRange(StreamUtils.ToBytes(ValuePropertyAtomObjectIDs[i]));
                }

                bytesList.AddRange(StreamUtils.ToBytes((int)0));

                return bytesList.ToArray();
            }
        }

        public NodePropertyTable(List<Int32> keyPropertyAtomObjectIDs, List<Int32> valuePropertyAtomObjectIDs)
        {
            KeyPropertyAtomObjectIDs = keyPropertyAtomObjectIDs;
            ValuePropertyAtomObjectIDs = valuePropertyAtomObjectIDs;
        }

        public NodePropertyTable(Stream stream)
        {
            KeyPropertyAtomObjectIDs = new List<Int32>();
            ValuePropertyAtomObjectIDs = new List<Int32>();

            var keyPropertyAtomObjectID = StreamUtils.ReadInt32(stream);

            while (keyPropertyAtomObjectID != 0)
            {
                KeyPropertyAtomObjectIDs.Add(keyPropertyAtomObjectID);
                ValuePropertyAtomObjectIDs.Add(StreamUtils.ReadInt32(stream));

                keyPropertyAtomObjectID = StreamUtils.ReadInt32(stream);
            }
        }
    }
}