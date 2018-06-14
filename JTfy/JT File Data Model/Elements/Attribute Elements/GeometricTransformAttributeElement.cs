using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class GeometricTransformAttributeElement : BaseAttributeElement
    {
        public UInt16 StoredValuesMask { get; protected set; }
        public Single[] ElementValues { get; protected set; }

        public readonly Single[] TransformationMatrix = ConstUtils.IndentityMatrix;

        public override int ByteCount
        {
            get
            {
                return base.ByteCount + 2 + ElementValues.Length * 4;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(StoredValuesMask));

                for (int i = 0, c = ElementValues.Length; i < c; ++i)
                {
                    bytesList.AddRange(StreamUtils.ToBytes(ElementValues[i]));
                }

                return bytesList.ToArray();
            }
        }

        public GeometricTransformAttributeElement(float[] transformationMatrix, int objectId)
            : base(objectId)
        {
            if (transformationMatrix == null) transformationMatrix = ConstUtils.IndentityMatrix;

            var transformationMatrixLength = transformationMatrix.Length;

            if (transformationMatrixLength < 16 || transformationMatrixLength > 16)
            {
                throw new Exception(String.Format("transformationMatrix has to be 16 floats long, currently it is {0} floats long", transformationMatrixLength));
            }

            var elementValueList = new List<Single>(16);

            StoredValuesMask = 0;

            var mask = (ushort)0x8000;

            for (int i = 0; i < transformationMatrixLength; ++i)
            {
                var value = transformationMatrix[i];

                if (value != ConstUtils.IndentityMatrix[i])
                {
                    elementValueList.Add(value);
                    StoredValuesMask |= mask;
                }

                mask >>= 1;
            }

            ElementValues = elementValueList.ToArray();

            TransformationMatrix = transformationMatrix;
        }

        public GeometricTransformAttributeElement(Stream stream)
            : base(stream)
        {
            StoredValuesMask = StreamUtils.ReadUInt16(stream);

            var elementValueList = new List<Single>(16);

            var storedValuesMask = StoredValuesMask;

            for (int i = 0, c = 16; i < c; ++i)
            {
                if ((storedValuesMask & 0x8000) > 0)
                {
                    var value = StreamUtils.ReadFloat(stream);

                    elementValueList.Add(value);

                    TransformationMatrix[i] = value;
                }

                storedValuesMask = (UInt16)(storedValuesMask << 1);
            }

            ElementValues = elementValueList.ToArray();
        }
    }
}