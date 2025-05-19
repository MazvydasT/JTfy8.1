namespace JTfy
{
    public class StringPropertyAtomElement : BasePropertyAtomElement
    {
        public MbString Value { get; private set; }

        public override int ByteCount
        {
            get
            {
                return base.ByteCount + Value.ByteCount;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(Value.Bytes);

                return bytesList.ToArray();
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public StringPropertyAtomElement(string value, int objectId) : this(new MbString(value), objectId) { }

        public StringPropertyAtomElement(MbString value, int objectId) : base(objectId)
        {
            Value = value;
        }

        public StringPropertyAtomElement(Stream stream)
            : base(stream)
        {
            Value = new MbString(stream);
        }
    }
}