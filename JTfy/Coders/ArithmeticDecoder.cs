using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JTfy
{
    public static class ArithmeticDecoder
    {
        public static Int32[] Decode(UInt32[] codeTextWords, Int32ProbabilityContexts int32ProbabilityContexts, Int32[] outOfBandValues, Int32 valueCount)
        {
            var bytes = new Byte[codeTextWords.Length * 4];
            Buffer.BlockCopy(codeTextWords, 0, bytes, 0, bytes.Length);

            var decodedSymbols = new List<Int32>(valueCount);

            var accumulatedProbabilityCounts = new AccumulatedProbabilityCounts(int32ProbabilityContexts);

            var code = 0x0000;
            var low = 0x0000;
            var high = 0xffff;
            var bitBuffer = 0;
            var bits = 0;
            var symbolCount = valueCount;
            var currentContext = 0;
            var newSymbolRange = new Int32[3];
            var outOfBandDataCounter = 0;

            using (var memoryStream = new MemoryStream(bytes))
            {
                var bitStream = new BitStream(memoryStream);

                GetNextCodeText(bitStream, out bitBuffer, out bits);

                code = (bitBuffer >> 16) & 0xffff;
                bitBuffer <<= 16;
                bits = 16;

                for (int i = 0; i < symbolCount; ++i)
                {
                    var rescaledCode = (((code - low) + 1) * accumulatedProbabilityCounts.SymbolCounts[currentContext] - 1) / ((high - low) + 1);
                    var int32ProbabilityContextTableEntry = GetEntryAndSymbolRangeByRescaledCode(accumulatedProbabilityCounts, int32ProbabilityContexts, currentContext, rescaledCode, ref newSymbolRange);

                    var range = high - low + 1;
                    high = low + ((range * newSymbolRange[1]) / newSymbolRange[2] - 1);
                    low += ((range * newSymbolRange[0]) / newSymbolRange[2]);

                    while (true)
                    {
                        if (((~(high ^ low)) & 0x8000) > 0)
                        {
                            //Shift both out if most sign. 
                        }

                        else if (((low & 0x4000) > 0) && ((high & 0x4000) == 0))
                        {
                            code ^= 0x4000;
                            code &= 0xffff;

                            low &= 0x3fff;
                            low &= 0xffff;

                            high |= 0x4000;
                            high &= 0xffff;
                        }

                        else
                        {
                            // Nothing to shift out any more

                            break;
                        }

                        low = (low << 1) & 0xffff;

                        high = (high << 1) & 0xffff;
                        high = (high | 1) & 0xffff;

                        code = (code << 1) & 0xffff;

                        if (bits == 0)
                        {
                            GetNextCodeText(bitStream, out bitBuffer, out bits);
                        }

                        code |= ((bitBuffer >> 31) & 0x00000001);
                        bitBuffer <<= 1;
                        bits--;
                    }

                    var symbol = int32ProbabilityContextTableEntry.Symbol;

                    if (symbol != -2 || currentContext <= 0)
                    {
                        if (symbol == -2 && outOfBandDataCounter >= outOfBandValues.Length)
                        {
                            throw new Exception("'Out-Of-Band' data missing! Read values: " + i + " / " + symbolCount);
                        }

                        decodedSymbols.Add(symbol == -2 && outOfBandDataCounter < outOfBandValues.Length ? outOfBandValues[outOfBandDataCounter++] : int32ProbabilityContextTableEntry.AssociatedValue);
                    }

                    currentContext = int32ProbabilityContextTableEntry.NextContext;
                }
            }

            return decodedSymbols.ToArray();
        }

        private static void GetNextCodeText(BitStream bitStream, out Int32 bitBuffer, out Int32 bits)
        {
            var nBits = (int)Math.Min(32, bitStream.Length - bitStream.Position);
            var uCodeText = bitStream.ReadAsUnsignedInt(nBits);

            if (nBits < 32) uCodeText <<= (32 - nBits);

            bitBuffer = uCodeText;
            bits = nBits;
        }

        private static Int32ProbabilityContextTableEntry GetEntryAndSymbolRangeByRescaledCode(AccumulatedProbabilityCounts accumulatedProbabilityCounts, Int32ProbabilityContexts int32ProbabilityContexts, Int32 contextIndex, Int32 rescaledCode, ref Int32[] newSymbolRange)
        {
            var entryByAccumulatedCountPerContext = accumulatedProbabilityCounts.EntriesByAccumulatedCountPerContext[contextIndex];

            var key = entryByAccumulatedCountPerContext.Keys.DefaultIfEmpty(-1).First(k => k > rescaledCode - 1);
            var value = entryByAccumulatedCountPerContext[key];

            var int32ProbabilityContextTableEntry = int32ProbabilityContexts.ProbabilityContextTableEntries[contextIndex][value];

            newSymbolRange[0] = key + 1 - int32ProbabilityContextTableEntry.OccurrenceCount;
            newSymbolRange[1] = key + 1;
            newSymbolRange[2] = accumulatedProbabilityCounts.SymbolCounts[contextIndex];

            return int32ProbabilityContextTableEntry;
        }
    }
}