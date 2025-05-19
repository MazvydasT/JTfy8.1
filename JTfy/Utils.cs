using ComponentAce.Compression.Libs.zlib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;

namespace JTfy
{
    public static class StreamUtils
    {
        public static bool DataIsLittleEndian { get; set; }

        public static Byte[] ReadBytes(Stream stream, int numberOfBytesToRead, bool checkEndianness = true)
        {
            var buffer = new Byte[numberOfBytesToRead];

            stream.ReadExactly(buffer, 0, numberOfBytesToRead);

            if (checkEndianness && BitConverter.IsLittleEndian != DataIsLittleEndian)
            {
                Array.Reverse(buffer);
            }

            return buffer;
        }

        private static readonly Dictionary<Type, byte> typeSizesInBytes = new Dictionary<Type, byte>()
        {
            {typeof(Byte),1},

            {typeof(UInt16),2},
            {typeof(Int16),2},

            {typeof(UInt32),4},
            {typeof(Int32),4},
            {typeof(Single),4},

            {typeof(UInt64),8},
            {typeof(Int64),8},
            {typeof(Double),8}
        };
        public static byte GetSizeInBytes(Type type)
        {
            if (typeSizesInBytes.TryGetValue(type, out byte returnValue)) return returnValue;

            throw new Exception(String.Format("Only Byte, UInt16, UInt32, UInt64, Int16, Int32, Int64, Single and Double types are allowed. Current type is {0}.", type.Name));
        }

        public static T Read<T>(Stream stream)
        {
            var numberOfBytesToRead = GetSizeInBytes(typeof(T));

            var resultArray = new T[1];

            Buffer.BlockCopy(ReadBytes(stream, numberOfBytesToRead), 0, resultArray, 0, numberOfBytesToRead);

            return resultArray[0];
        }

        public static Byte ReadByte(Stream stream) { return Read<Byte>(stream); }

        public static UInt16 ReadUInt16(Stream stream) { return Read<UInt16>(stream); }
        public static UInt32 ReadUInt32(Stream stream) { return Read<UInt32>(stream); }
        public static UInt64 ReadUInt64(Stream stream) { return Read<UInt64>(stream); }

        public static Int16 ReadInt16(Stream stream) { return Read<Int16>(stream); }
        public static Int32 ReadInt32(Stream stream) { return Read<Int32>(stream); }
        public static Int64 ReadInt64(Stream stream) { return Read<Int64>(stream); }

        public static Single ReadFloat(Stream stream) { return Read<Single>(stream); }
        public static Double ReadDouble(Stream stream) { return Read<Double>(stream); }

        public static Byte[] ToBytes<T>(T value)
        {
            var numberOfBytesToRead = GetSizeInBytes(typeof(T));

            Byte[] bytes = new Byte[numberOfBytesToRead];

            Buffer.BlockCopy(new T[] { value }, 0, bytes, 0, numberOfBytesToRead);

            if (DataIsLittleEndian != BitConverter.IsLittleEndian) Array.Reverse(bytes);

            return bytes;
        }

        private static byte[] CheckEndianness(byte[] bytes)
        {
            if (DataIsLittleEndian != BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return bytes;
        }
        public static byte[] ToBytes(byte value) { return new byte[] { value }; }

        public static byte[] ToBytes(ushort value) { return CheckEndianness(BitConverter.GetBytes(value)); }
        public static byte[] ToBytes(short value) { return CheckEndianness(BitConverter.GetBytes(value)); }

        public static byte[] ToBytes(uint value) { return CheckEndianness(BitConverter.GetBytes(value)); }
        public static byte[] ToBytes(int value) { return CheckEndianness(BitConverter.GetBytes(value)); }
        public static byte[] ToBytes(float value) { return CheckEndianness(BitConverter.GetBytes(value)); }

        public static byte[] ToBytes(ulong value) { return CheckEndianness(BitConverter.GetBytes(value)); }
        public static byte[] ToBytes(long value) { return CheckEndianness(BitConverter.GetBytes(value)); }
        public static byte[] ToBytes(double value) { return CheckEndianness(BitConverter.GetBytes(value)); }
    }

    public static class ConvUtils<U>
    {
        public static T To<T>(U value)
        {
            T[] result = new T[1];

            Buffer.BlockCopy(new U[] { value }, 0, result, 0, StreamUtils.GetSizeInBytes(typeof(U)));

            return result[0];
        }
    }

    public static class ConstUtils
    {
        public const string VariantStringEnding = " \n\r\n ";
        public const int VariantStringRequiredLength = 80;

        public const string EndOfElementAsString = "{0xffffffff,0xffff,0xffff,{0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff}}";
        public static readonly GUID EndOfElement = new GUID(EndOfElementAsString);

        public static float[] IndentityMatrix
        {
            get
            {
                return new Single[]
                {
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                };
            }
        }

        public static readonly Dictionary<string, Tuple<Type, byte>> ObjectTypeIdToType = new Dictionary<string, Tuple<Type, byte>>()
        {
            // LSG Segment Graph elements
            {"{0x10dd102a,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(InstanceNodeElement), 0)},
            {"{0x10dd103e,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(PartitionNodeElement), 1)},
            {"{0xce357245,0x38fb,0x11d1,{0xa5,0x06,0x00,0x60,0x97,0xbd,0xc6,0xe1}}", new Tuple<Type, byte>(typeof(MetaDataNodeElement), 1)},
            {"{0xce357244,0x38fb,0x11d1,{0xa5,0x06,0x00,0x60,0x97,0xbd,0xc6,0xe1}}", new Tuple<Type, byte>(typeof(PartNodeElement), 1)},
            {"{0x10dd104c,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(RangeLODNodeElement), 1)},
            {"{0x10dd101b,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(GroupNodeElement), 1)},
            {"{0x10dd1077,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(TriStripSetShapeNodeElement), 2)},
            {"{0x10dd1030,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(MaterialAttributeElement), 3)},
            {"{0x10dd1083,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(GeometricTransformAttributeElement), 3)},

            // LSG Segment Property Atom elements
            {"{0x10dd106e,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(StringPropertyAtomElement), 5)},
            {"{0x10dd1019,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(FloatingPointPropertyAtomElement), 5)},
            {"{0x10dd102b,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(IntegerPropertyAtomElement), 5)},
            {"{0xce357246,0x38fb,0x11d1,{0xa5,0x06,0x00,0x60,0x97,0xbd,0xc6,0xe1}}", new Tuple<Type, byte>(typeof(DatePropertyAtomElement), 5)},
            {"{0xe0b05be5,0xfbbd,0x11d1,{0xa3,0xa7,0x00,0xaa,0x00,0xd1,0x09,0x54}}", new Tuple<Type, byte>(typeof(LateLoadedPropertyAtomElement), 8)},

            // Meta Data Segment elements
            {"{0xce357247,0x38fb,0x11d1,{0xa5,0x06,0x00,0x60,0x97,0xbd,0xc6,0xe1}}", new Tuple<Type, byte>(typeof(PropertyProxyMetaDataElement), 9)},

            // Shape LOD Segment
            {"{0x10dd10ab,0x2ac8,0x11d1,{0x9b,0x6b,0x00,0x80,0xc7,0xbb,0x59,0x97}}", new Tuple<Type, byte>(typeof(TriStripSetShapeLODElement), 4)}
        };

        public static readonly Dictionary<Type, Tuple<string, byte>> TypeToObjectTypeId;

        static ConstUtils()
        {
            TypeToObjectTypeId = new Dictionary<Type, Tuple<string, byte>>(ObjectTypeIdToType.Count);

            foreach (var entry in ObjectTypeIdToType)
            {
                TypeToObjectTypeId[entry.Value.Item1] = new Tuple<string, byte>(entry.Key, entry.Value.Item2);
            }
        }
    }

    public static class CompressionUtils
    {
        public static Byte[] Compress(Byte[] data)
        {
            using (var compressedDataStream = new MemoryStream())
            using (var zOutputStream = new ZOutputStream(compressedDataStream, zlibConst.Z_BEST_COMPRESSION))
            //using (var zOutputStream = new zlib.ZOutputStream(compressedDataStream, 9))
            {
                zOutputStream.Write(data, 0, data.Length);
                zOutputStream.Flush();
                zOutputStream.finish();

                return compressedDataStream.ToArray();
            }
        }

        public static Byte[] Decompress(Byte[] data)
        {
            using (var decompressedDataStream = new MemoryStream())
            using (var zOutputStream = new ZOutputStream(decompressedDataStream))
            {
                zOutputStream.Write(data, 0, data.Length);
                zOutputStream.Flush();
                zOutputStream.finish();

                return decompressedDataStream.ToArray();
            }
        }
    }

    public static class IdGenUtils
    {
        private static Int32 id = 0;
        public static Int32 NextId
        {
            get { return ++id; }
        }
    }

    public static class RandomGenUtils
    {
        private static readonly RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

        public static double NextDouble(double min, double max)
        {
            var bytes = new byte[8];

            random.GetBytes(bytes);

            var uLongArray = new ulong[1];
            Buffer.BlockCopy(bytes, 0, uLongArray, 0, bytes.Length);

            var randomValue = (double)uLongArray[0] / ulong.MaxValue;

            var result = randomValue * (max - min) + min;

            return result;
        }

        public static Color NextColour()
        {
            var minSV = 0.4d;

            var h = NextDouble(0, 360);
            var s = NextDouble(minSV, 1);
            var v = NextDouble(minSV, 1);

            return ColourUtils.HSV2RGB(h, s, v);
        }
    }

    public static class ColourUtils
    {
        public static Color HSV2RGB(double h, double S, double V)
        {
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }

            return Color.FromArgb(Clamp((int)(R * 255.0)), Clamp((int)(G * 255.0)), Clamp((int)(B * 255.0)));
        }

        /// <summary>
        /// Clamp a value to 0-255
        /// </summary>
        static int Clamp(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }
    }

    public static class CalcUtils
    {
        public readonly struct Point3D(float x, float y, float z)
        {
            public float X { get; } = x; public float Y { get; } = y; public float Z { get; } = z;
        }

        public static double GetTriangleArea(float[] point1, float[] point2, float[] point3)
        {
            return GetTriangleArea(
                new Point3D(point1[0], point1[1], point1[2]),
                new Point3D(point2[0], point2[1], point2[2]),
                new Point3D(point3[0], point3[1], point3[2]));
        }

        public static double GetTriangleArea(Point3D point1, Point3D point2, Point3D point3)
        {
            double a = GetPointToPointDistance(point2, point1);
            double b = GetPointToPointDistance(point3, point2);
            double c = GetPointToPointDistance(point1, point3);
            double s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }

        public static double GetPointToPointDistance(Point3D point1, Point3D point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2) + Math.Pow(point2.Z - point1.Z, 2));
        }
    }
}