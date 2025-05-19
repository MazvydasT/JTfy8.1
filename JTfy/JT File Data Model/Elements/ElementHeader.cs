namespace JTfy
{
    public class ElementHeader : BaseDataStructure
    {
        public int ElementLength { get; protected set; }
        public GUID ObjectTypeID { get; protected set; }
        public byte ObjectBaseType { get; protected set; }

        public static int Size { get { return 4 + GUID.Size + 1; } }

        public override int ByteCount { get { return Size; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(ElementLength));
                bytesList.AddRange(ObjectTypeID.Bytes);
                bytesList.Add(ObjectBaseType);

                return bytesList.ToArray();
            }
        }

        public ElementHeader(int elementLength, GUID objectTypeID, Byte objectBaseType)
        {
            ElementLength = elementLength;
            ObjectTypeID = objectTypeID;
            ObjectBaseType = objectBaseType;
        }

        public ElementHeader(Stream stream)
        {
            ElementLength = StreamUtils.ReadInt32(stream);
            ObjectTypeID = new GUID(stream);

            if (ObjectTypeID.ToString() != ConstUtils.EndOfElementAsString)
            {
                ObjectBaseType = StreamUtils.ReadByte(stream);
            }
        }
    }
}