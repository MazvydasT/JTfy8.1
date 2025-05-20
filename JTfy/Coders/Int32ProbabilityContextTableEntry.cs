namespace JTfy
{
    public class Int32ProbabilityContextTableEntry(BitStream bitStream, Int32 numberOfSymbolBits, Int32 numberOfOccurrenceCountBits, Int32 numberOfAssociatedValueBits, Int32 numberOfNextContextBits, Int32 minimumValue) : BaseDataStructure
    {
        public Int32 Symbol { get; protected set; } = bitStream.ReadAsUnsignedInt(numberOfSymbolBits) - 2;
        public Int32 OccurrenceCount { get; protected set; } = bitStream.ReadAsUnsignedInt(numberOfOccurrenceCountBits);
        public Int32 AssociatedValue { get; protected set; } = bitStream.ReadAsUnsignedInt(numberOfAssociatedValueBits) + minimumValue;
        public Int32 NextContext { get; protected set; } = numberOfNextContextBits != -1 ? bitStream.ReadAsUnsignedInt(numberOfNextContextBits) : 0;

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
    }
}