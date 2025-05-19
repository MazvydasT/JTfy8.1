namespace JTfy
{
    public class BBoxF32 : BaseDataStructure
    {
        public CoordF32 MinCorner { get; private set; }
        public CoordF32 MaxCorner { get; private set; }

        public override int ByteCount
        {
            get
            {
                return 2 * MaxCorner.ByteCount;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(MinCorner.Bytes);
                bytesList.AddRange(MaxCorner.Bytes);

                return bytesList.ToArray();
            }
        }

        public BBoxF32(Stream stream)
        {
            MinCorner = new CoordF32(stream);
            MaxCorner = new CoordF32(stream);
        }

        public BBoxF32(CoordF32 minCorner, CoordF32 maxCorner)
        {
            MinCorner = minCorner;
            MaxCorner = maxCorner;
        }

        public BBoxF32() : this(new CoordF32(), new CoordF32()) { }
        public BBoxF32(float xMin, float yMin, float zMin, float xMax, float yMax, float zMax) : this(new CoordF32(xMin, yMin, zMin), new CoordF32(xMax, yMax, zMax)) { }
    }
}