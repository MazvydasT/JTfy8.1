namespace JTfy
{
    public class FloatingPointPropertyAtomElement : BasePropertyAtomElement
    {
        public float Value { get; private set; }

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

        public FloatingPointPropertyAtomElement(float value, int objectId)
            : base(objectId)
        {
            Value = value;
        }

        public FloatingPointPropertyAtomElement(Stream stream)
            : base(stream)
        {
            Value = StreamUtils.ReadFloat(stream);
        }
    }
}