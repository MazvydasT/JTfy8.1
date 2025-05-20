namespace JTfy
{
    public class AccumulatedProbabilityCounts
    {
        //private readonly Int32ProbabilityContexts int32ProbabilityContexts;
        public Int32[] SymbolCounts { get; private set; }
        public SortedDictionary<Int32, Int32>[] EntriesByAccumulatedCountPerContext { get; private set; }

        public AccumulatedProbabilityCounts(Int32ProbabilityContexts int32ProbabilityContexts)
        {
            //this.int32ProbabilityContexts = int32ProbabilityContexts;
            SymbolCounts = new Int32[int32ProbabilityContexts.ProbabilityContextTableEntries.Length];
            EntriesByAccumulatedCountPerContext = new SortedDictionary<int, int>[SymbolCounts.Length];

            for (int tableIndex = 0, tableCount = SymbolCounts.Length; tableIndex < tableCount; ++tableIndex)
            {
                var table = int32ProbabilityContexts.ProbabilityContextTableEntries[tableIndex];

                int accumulatedCount = 0;

                var entryByAccumulatedCountPerContext = new SortedDictionary<int, int>();

                EntriesByAccumulatedCountPerContext[tableIndex] = entryByAccumulatedCountPerContext;

                for (int tableEntryIndex = 0, tableEntryCount = table.Length; tableEntryIndex < tableEntryCount; ++tableEntryIndex)
                {
                    accumulatedCount += table[tableEntryIndex].OccurrenceCount;
                    entryByAccumulatedCountPerContext[accumulatedCount - 1] = tableEntryIndex;
                }

                SymbolCounts[tableIndex] = accumulatedCount;
            }
        }
    }
}
