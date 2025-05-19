using System.Drawing;

namespace JTfy
{
    public class RGB : DataArray<Single>
    {
        public Single Red { get { return data[0]; } }
        public Single Green { get { return data[1]; } }
        public Single Blue { get { return data[2]; } }

        public override string ToString()
        {
            return String.Format("R:{0}-G:{1}-B:{2}", Red, Green, Blue);
        }

        public RGB(Stream stream)
            : base()
        {
            data =
            [
                StreamUtils.ReadFloat(stream),
                StreamUtils.ReadFloat(stream),
                StreamUtils.ReadFloat(stream)
            ];
        }

        public RGB(Single red, Single green, Single blue)
        {
            data =
            [
                red,
                green,
                blue
            ];
        }
    }

    public class RGBA : RGB
    {
        public Single Alpha { get { return data[3]; } }

        public override string ToString()
        {
            return String.Format("A:{0}-{1}", Alpha, base.ToString());
        }

        public RGBA(Stream stream)
            : base(stream)
        {
            data =
            [
                base.Red,
                base.Green,
                base.Blue,
                StreamUtils.ReadFloat(stream)
            ];
        }

        public RGBA(Single red, Single green, Single blue, Single alpha)
            : base(red, green, blue)
        {
            data =
            [
                base.Red,
                base.Green,
                base.Blue,
                alpha
            ];
        }

        public RGBA() : this(0, 0, 0, 1) { }
        public RGBA(Color colour) : this(Convert(colour.R), Convert(colour.G), Convert(colour.B), Convert(colour.A)) { }

        private static float Convert(byte colourComponentValue)
        {
            return (float)colourComponentValue / (float)255;
        }
    }
}