namespace JTfy
{
    public class TOCSegment : BaseDataStructure
    {
        public int EntryCount { get; private set; }
        public TOCEntry[] TOCEntries { get; private set; }

        public override int ByteCount { get { return 4 + EntryCount * TOCEntry.Size; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(EntryCount));

                for (int i = 0; i < EntryCount; ++i)
                {
                    bytesList.AddRange(TOCEntries[i].Bytes);
                }

                return bytesList.ToArray();
            }
        }

        public TOCSegment(TOCEntry[] tocEntries)
        {
            EntryCount = tocEntries.Length;
            TOCEntries = tocEntries;
        }

        public TOCSegment(Stream stream)
        {
            EntryCount = StreamUtils.ReadInt32(stream);

            TOCEntries = new TOCEntry[EntryCount];

            for (int i = 0; i < EntryCount; ++i)
            {
                TOCEntries[i] = new TOCEntry(stream);
            }
        }
    }
}