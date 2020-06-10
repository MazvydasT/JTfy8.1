using System;
using System.Collections.Generic;
using System.IO;
//using System.Linq;

namespace JTfy
{
    public static class BitlengthCoder
    {
        public static Int32[] Decode(uint[] codeTextWords, int valueCount, int codeTextLength)
        {
            var bytes = new Byte[codeTextWords.Length * 4];
            Buffer.BlockCopy(codeTextWords, 0, bytes, 0, bytes.Length);

            var decodedSymbols = new List<Int32>(valueCount);

            var bitFieldWidth = 0;

            using (var memoryStream = new MemoryStream(bytes))
            {
                var bitStream = new BitStream(memoryStream);

                while (bitStream.Position != codeTextLength)
                {
                    if (bitStream.ReadAsUnsignedInt(1) != 0)
                    {
                        var adjustmentBit = bitStream.ReadAsUnsignedInt(1);

                        do
                        {
                            if (adjustmentBit == 1)
                                bitFieldWidth += 2;
                            else
                                bitFieldWidth -= 2;
                        }
                        while (bitStream.ReadAsUnsignedInt(1) == adjustmentBit);
                    }

                    int decodedSymbol;

                    if (bitFieldWidth == 0)
                    {
                        decodedSymbol = 0;
                    }

                    else
                    {
                        decodedSymbol = bitStream.ReadAsUnsignedInt(bitFieldWidth);
                        decodedSymbol <<= (32 - bitFieldWidth);
                        decodedSymbol >>= (32 - bitFieldWidth);
                    }

                    decodedSymbols.Add(decodedSymbol);
                }
            }

            return decodedSymbols.ToArray();
        }

        /*private static Int32 GetBitFieldWidth(Int32 symbol)
        {
            if (symbol == 0) return 0;

            symbol = Math.Abs(symbol);

            Int32 i, bitFieldWidth;

            for (i = 1, bitFieldWidth = 0; i <= symbol && bitFieldWidth < 31; i += i, bitFieldWidth++) { }

            return bitFieldWidth;
        }*/
    }
}