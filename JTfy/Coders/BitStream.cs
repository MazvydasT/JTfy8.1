using System.Collections;

namespace JTfy
{
    public class BitStream
    {
        private readonly Stream stream;
        public long Length { get; private set; }
        public long Position { get; private set; }

        private readonly bool[] buffer = new bool[8];
        private byte bufferPosition;

        private bool initialised = false;

        public BitStream(Stream stream)
        {
            this.stream = stream;
            this.Length = stream.Length << 3; // same as stream.Length * 8 but faster
            this.Position = stream.Position << 3; // same as stream.Position * 8 but faster
        }

        private bool ReadBit()
        {
            if (!initialised)
            {
                new BitArray([StreamUtils.ReadByte(stream)]).CopyTo(buffer, 0);
                Array.Reverse(buffer);

                bufferPosition = 0;

                initialised = true;
            }

            if (Position >= Length)
            {
                throw new Exception("Cannot read past end of stream.");
            }

            if (bufferPosition == buffer.Length)
            {
                new BitArray([StreamUtils.ReadByte(stream)]).CopyTo(buffer, 0);
                Array.Reverse(buffer);

                bufferPosition = 0;
            }

            ++Position;

            return buffer[bufferPosition++];
        }

        private bool[] ReadBits(int numberOfBitsToRead)
        {
            var bitStack = new Stack<bool>(numberOfBitsToRead);

            for (int i = 0; i < numberOfBitsToRead; ++i)
            {
                bitStack.Push(ReadBit());
            }

            return [.. bitStack];
        }

        public Int32 ReadAsUnsignedInt(int numberOfBitsToRead)
        {
            var bytes = new byte[4];

            new BitArray(ReadBits(numberOfBitsToRead)).CopyTo(bytes, 0);

            var result = new Int32[1];

            Buffer.BlockCopy(bytes, 0, result, 0, bytes.Length);

            return result[0];
        }

        public Int32 ReadAsSignedInt(int numberOfBitsToRead)
        {
            var result = ReadAsUnsignedInt(numberOfBitsToRead);

            result <<= (32 - numberOfBitsToRead);
            result >>= (32 - numberOfBitsToRead);

            return result;
        }
    }
}
