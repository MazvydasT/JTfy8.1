using System.Text;

namespace JTfy
{
    public class FileHeader : BaseDataStructure
    {
        public Byte[] Version { get; protected set; }
        public Byte ByteOrder { get; protected set; }
        public Int32 ReservedField { get; protected set; }
        public Int32 TOCOffset { get; set; }
        public GUID LSGSegmentID { get; protected set; }

        public string VersionAsString
        {
            get
            {
                return Encoding.ASCII.GetString(Version);
            }
        }

        public static int Size
        {
            get { return 80 + 1 + 4 + 4 + GUID.Size; }
        }

        public override int ByteCount
        {
            get { return Size; }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(Version);
                bytesList.Add(ByteOrder);
                bytesList.AddRange(StreamUtils.ToBytes(ReservedField));
                bytesList.AddRange(StreamUtils.ToBytes(TOCOffset));
                bytesList.AddRange(LSGSegmentID.Bytes);

                return bytesList.ToArray();
            }
        }

        public FileHeader(string version, Byte byteOrder, Int32 tocOffset, GUID lsgSegmentID)
        {
            var versionStringLength = version.Length;

            if (versionStringLength > ConstUtils.VariantStringRequiredLength || (version.EndsWith(ConstUtils.VariantStringEnding) && versionStringLength < ConstUtils.VariantStringRequiredLength))
            {
                throw new Exception(String.Format("Version string has to be {0} characters long, currently it is {1} characters long", ConstUtils.VariantStringRequiredLength, versionStringLength));
            }

            else if (versionStringLength < 76)
            {
                version = version.PadRight(75, ' ') + ConstUtils.VariantStringEnding;
            }

            Version = Encoding.ASCII.GetBytes(version);
            ByteOrder = byteOrder;

            StreamUtils.DataIsLittleEndian = ByteOrder == 0;

            ReservedField = 0;
            TOCOffset = tocOffset;
            LSGSegmentID = lsgSegmentID;
        }

        public FileHeader(Stream stream)
        {
            Version = StreamUtils.ReadBytes(stream, 80, false);
            ByteOrder = StreamUtils.ReadByte(stream);

            StreamUtils.DataIsLittleEndian = ByteOrder == 0;

            ReservedField = StreamUtils.ReadInt32(stream);
            TOCOffset = StreamUtils.ReadInt32(stream);
            LSGSegmentID = new GUID(stream);
        }
    }
}