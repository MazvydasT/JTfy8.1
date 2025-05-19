namespace JTfy
{
    public class LogicElementHeaderZLIB : BaseDataStructure
    {
        public Int32 CompressionFlag { get; protected set; }
        public Int32 CompressedDataLength { get; protected set; }
        public Byte CompressionAlgorithm { get; protected set; }

        public override int ByteCount
        {
            get { return 4 + 4 + 1; }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesArray = new List<byte>(ByteCount);

                bytesArray.AddRange(StreamUtils.ToBytes(CompressionFlag));
                bytesArray.AddRange(StreamUtils.ToBytes(CompressedDataLength));
                bytesArray.Add(CompressionAlgorithm);

                return bytesArray.ToArray();
            }
        }

        public LogicElementHeaderZLIB(Int32 compressionFlag, Int32 compressedDataLength, Byte compressionAlgorithm)
        {
            CompressionFlag = compressionFlag;
            CompressedDataLength = compressedDataLength;
            CompressionAlgorithm = compressionAlgorithm;
        }

        public LogicElementHeaderZLIB(Stream stream)
        {
            CompressionFlag = StreamUtils.ReadInt32(stream);
            CompressedDataLength = StreamUtils.ReadInt32(stream);
            CompressionAlgorithm = StreamUtils.ReadByte(stream);
        }
    }
}