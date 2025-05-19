namespace JTfy
{
    public class IntegerPropertyAtomElement : BasePropertyAtomElement
    {
        public int Value { get; private set; }

        public override int ByteCount
        {
            get
            {
                return base.ByteCount + 4;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(Value));

                return bytesList.ToArray();
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public IntegerPropertyAtomElement(int value, int objectId)
            : base(objectId)
        {
            Value = value;
        }

        public IntegerPropertyAtomElement(Stream stream)
            : base(stream)
        {
            Value = StreamUtils.ReadInt32(stream);
        }
    }
}