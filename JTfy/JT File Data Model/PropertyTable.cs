namespace JTfy
{
    public class PropertyTable
    {
        public Int16 VersionNumber { get; protected set; }
        public int NodePropertyTableCount { get; protected set; }

        public int[] NodeObjectIDs { get; protected set; }
        public NodePropertyTable[] NodePropertyTables { get; protected set; }

        public Int32 ByteCount
        {
            get
            {
                int size = 2 + 4 + NodePropertyTableCount * 4;

                foreach (var elementPropertyTable in NodePropertyTables)
                {
                    size += elementPropertyTable.ByteCount;
                }

                return size;
            }
        }

        public Byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(VersionNumber));
                bytesList.AddRange(StreamUtils.ToBytes(NodePropertyTableCount));

                for (int i = 0; i < NodePropertyTableCount; ++i)
                {
                    bytesList.AddRange(StreamUtils.ToBytes(NodeObjectIDs[i]));
                    bytesList.AddRange(NodePropertyTables[i].Bytes);
                }

                return bytesList.ToArray();
            }
        }

        public PropertyTable(Int32[] elementObjectIDs, NodePropertyTable[] elementPropertyTables)
        {
            VersionNumber = 1;
            NodePropertyTableCount = elementObjectIDs.Length;
            NodeObjectIDs = elementObjectIDs;
            NodePropertyTables = elementPropertyTables;
        }

        public PropertyTable(Stream stream)
        {
            VersionNumber = StreamUtils.ReadInt16(stream);
            NodePropertyTableCount = StreamUtils.ReadInt32(stream);

            NodeObjectIDs = new Int32[NodePropertyTableCount];
            NodePropertyTables = new NodePropertyTable[NodePropertyTableCount];

            for (int i = 0; i < NodePropertyTableCount; ++i)
            {
                NodeObjectIDs[i] = StreamUtils.ReadInt32(stream);
                NodePropertyTables[i] = new NodePropertyTable(stream);
            }
        }
    }
}
