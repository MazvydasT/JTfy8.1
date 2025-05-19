namespace JTfy
{
    public class Int32ProbabilityContexts : BaseDataStructure
    {
        //public Byte ProbabilityContextTableCount { get; protected set; } // For some reason this value is not present in JT file and only 1 ProbabilityContextTable is read from the file.
        public Int32ProbabilityContextTableEntry[][] ProbabilityContextTableEntries { get; protected set; }

        public override int ByteCount
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override byte[] Bytes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /*public Int32ProbabilityContexts(Stream stream)
        {
            var symbol2AssociatedValueMap = new Dictionary<int, int>();

            int probabilityContextTableCount = StreamUtils.ReadByte(stream);

            int numberSymbolBits = -1;
            int numberOccurrenceCountBits = -1;
            int numberValueBits = -1;
            int numberNextContextBits = -1;
            int minimumValue = -1;

            var bitStream = new BitStream(stream);

            var int32ProbabilityContextTableEntries = new List<Int32ProbabilityContextTableEntry>[probabilityContextTableCount];
            for (int i = 0; i < probabilityContextTableCount; ++i)
            {
                int32ProbabilityContextTableEntries[i] = new List<Int32ProbabilityContextTableEntry>();

                var probabilityContextTableEntryCount = bitStream.readAsSignedInt(32);
            }
        }*/
    }
}
