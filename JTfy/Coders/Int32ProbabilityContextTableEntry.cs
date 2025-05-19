namespace JTfy
{
    public class Int32ProbabilityContextTableEntry : BaseDataStructure
    {
        public Int32 Symbol { get; protected set; }
        public Int32 OccurrenceCount { get; protected set; }
        public Int32 AssociatedValue { get; protected set; }
        public Int32 NextContext { get; protected set; }

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

        public Int32ProbabilityContextTableEntry(BitStream bitStream, Int32 numberOfSymbolBits, Int32 numberOfOccurrenceCountBits, Int32 numberOfAssociatedValueBits, Int32 numberOfNextContextBits, Int32 minimumValue)
        {
            Symbol = bitStream.ReadAsUnsignedInt(numberOfSymbolBits) - 2;
            OccurrenceCount = bitStream.ReadAsUnsignedInt(numberOfOccurrenceCountBits);
            AssociatedValue = bitStream.ReadAsUnsignedInt(numberOfAssociatedValueBits) + minimumValue;
            NextContext = numberOfNextContextBits != -1 ? bitStream.ReadAsUnsignedInt(numberOfNextContextBits) : 0;
        }
    }
}