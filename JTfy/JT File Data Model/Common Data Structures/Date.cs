namespace JTfy
{
    public class Date : DataArray<Int16>
    {
        public Int16 Year { get { return data[0]; } }
        public Int16 Month { get { return data[1]; } }
        public Int16 Day { get { return data[2]; } }
        public Int16 Hour { get { return data[3]; } }
        public Int16 Minute { get { return data[4]; } }
        public Int16 Second { get { return data[5]; } }

        public override string ToString()
        {
            return new DateTime(Year, Month + 1, Day, Hour, Minute, Second).ToString();
        }

        public Date(Stream stream)
        {
            data = new Int16[6];

            for (int i = 0, c = data.Length; i < c; ++i)
            {
                data[i] = StreamUtils.ReadInt16(stream);
            }
        }

        public Date(Int16 year, Int16 month, Int16 day, Int16 hour, Int16 minute, Int16 second)
        {
            data = new Int16[]
            {
                year,
                month,
                day,
                hour,
                minute,
                second
            };
        }

        public Date(DateTime date) : this((Int16)date.Year, (Int16)(date.Month - 1), (Int16)date.Day, (Int16)date.Hour, (Int16)date.Minute, (Int16)date.Second) { }
    }
}