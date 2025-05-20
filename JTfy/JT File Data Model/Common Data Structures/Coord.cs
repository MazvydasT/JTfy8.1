namespace JTfy
{
    public class Coord<T> : DataArray<T>
    {
        public Coord(Stream stream)
        {
            data = new T[3];

            for (int i = 0, c = data.Length; i < c; ++i)
            {
                data[i] = StreamUtils.Read<T>(stream);
            }
        }

        public Coord(T x, T y, T z)
            : base()
        {
            data =
            [
                x,
                y,
                z
            ];
        }
    }

    public class CoordF32 : Coord<Single>
    {
        public float X { get { return data[0]; } }
        public float Y { get { return data[1]; } }
        public float Z { get { return data[2]; } }

        public CoordF32(Stream stream) : base(stream) { }

        public CoordF32(float x, float y, float z) : base(x, y, z) { }

        public CoordF32() : this(0, 0, 0) { }
    }
}