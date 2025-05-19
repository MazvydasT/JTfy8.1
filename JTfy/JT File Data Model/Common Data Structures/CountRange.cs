namespace JTfy
{
    public class CountRange : DataArray<int>
    {
        public int Min { get { return data[0]; } }
        public int Max { get { return data[1]; } }

        public CountRange(Stream stream)
        {
            data = new int[]
            {
                StreamUtils.ReadInt32(stream),
                StreamUtils.ReadInt32(stream)
            };
        }

        public CountRange(int minCount, int maxCount)
        {
            data = new int[]
            {
                minCount,
                maxCount
            };
        }

        public CountRange() : this(0, 0) { }
        public CountRange(int count) : this(count, count) { }
    }
}