using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace JTfy
{
    public class MaterialAttributeElement : BaseAttributeElement
    {
        public UInt16 DataFlags { get; set; }

        public float AmbientCommonRGBValue { get; protected set; }
        public RGBA AmbientColour { get; set; }

        public RGBA DiffuseColour { get; set; }

        public float SpecularCommonRGBValue { get; protected set; }
        public RGBA SpecularColour { get; set; }

        public float EmissionCommonRGBValue { get; protected set; }
        public RGBA EmissionColour { get; set; }

        private float shininess = 30;
        public float Shininess { get { return shininess; } set { shininess = value < 0 ? 0 : (value > 128 ? 128 : value); } }

        public override int ByteCount
        {
            get
            {
                return base.ByteCount + 2 + (AmbientColour == null ? 4 : AmbientColour.ByteCount) + DiffuseColour.ByteCount + (SpecularColour == null ? 4 : SpecularColour.ByteCount) + (EmissionColour == null ? 4 : EmissionColour.ByteCount) + 4;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(DataFlags));

                bytesList.AddRange(AmbientColour == null ? StreamUtils.ToBytes(AmbientCommonRGBValue) : AmbientColour.Bytes);

                bytesList.AddRange(DiffuseColour.Bytes);

                bytesList.AddRange(SpecularColour == null ? StreamUtils.ToBytes(SpecularCommonRGBValue) : SpecularColour.Bytes);
                bytesList.AddRange(EmissionColour == null ? StreamUtils.ToBytes(EmissionCommonRGBValue) : EmissionColour.Bytes);

                bytesList.AddRange(StreamUtils.ToBytes(Shininess));

                return bytesList.ToArray();
            }
        }

        /*public override string ToString()
        {
            return String.Join("|", new object[]
            {
                DataFlags,
                AmbientColour == null ? AmbientCommonRGBValue : (object)AmbientColour,
                DiffuseColour,
                SpecularColour == null ? SpecularCommonRGBValue : (object)SpecularColour,
                EmissionColour == null ? EmissionCommonRGBValue : (object)EmissionColour,
                Shininess
            });
        }*/

        public MaterialAttributeElement(Color diffuseColour, int objectId) : this(new RGBA(diffuseColour), new RGBA(diffuseColour), new RGBA(diffuseColour), new RGBA(), objectId) { }

        public MaterialAttributeElement(RGBA ambientColour, RGBA diffuseColour, RGBA specularColour, RGBA emissionColour, int objectId)
            : base(objectId)
        {
            DataFlags = 0;

            DataFlags |= 0x0010; // Blending Flag - Blending ON
            DataFlags |= (6 << 6); // Source Blend Factor - GL_SRC_ALPHA
            DataFlags |= (7 << 11); // Destination Blend Factor - GL_ONE_MINUS_SRC_ALPHA

            if (ambientColour == null) ambientColour = new RGBA();
            if (ambientColour.Red == ambientColour.Green && ambientColour.Red == ambientColour.Blue && ambientColour.Alpha == 1)
            {
                DataFlags |= 0x0001;
                DataFlags |= 0x0002;

                AmbientCommonRGBValue = ambientColour.Red;
            }

            else
            {
                AmbientColour = ambientColour;
            }

            if (diffuseColour == null) diffuseColour = new RGBA();
            DiffuseColour = diffuseColour;

            if (specularColour == null) specularColour = new RGBA();
            if (specularColour.Red == specularColour.Green && specularColour.Red == specularColour.Blue && specularColour.Alpha == 1)
            {
                DataFlags |= 0x0001;
                DataFlags |= 0x0008;

                SpecularCommonRGBValue = specularColour.Red;
            }

            else
            {
                SpecularColour = specularColour;
            }

            if (emissionColour == null) emissionColour = new RGBA();
            if (emissionColour.Red == emissionColour.Green && emissionColour.Red == emissionColour.Blue && emissionColour.Alpha == 1)
            {
                DataFlags |= 0x0001;
                DataFlags |= 0x0004;

                EmissionCommonRGBValue = emissionColour.Red;
            }

            else
            {
                EmissionColour = emissionColour;
            }

            Shininess = shininess;
        }

        public MaterialAttributeElement(Stream stream)
            : base(stream)
        {
            DataFlags = StreamUtils.ReadUInt16(stream);

            var patternBitsAreUsed = (DataFlags & 0x0001) > 0;

            if (patternBitsAreUsed && (DataFlags & 0x0002) > 0)
            {
                AmbientCommonRGBValue = StreamUtils.ReadFloat(stream);
            }

            else
            {
                AmbientColour = new RGBA(stream);
            }

            DiffuseColour = new RGBA(stream);

            if (patternBitsAreUsed && (DataFlags & 0x0008) > 0)
            {
                SpecularCommonRGBValue = StreamUtils.ReadFloat(stream);
            }

            else
            {
                SpecularColour = new RGBA(stream);
            }

            if (patternBitsAreUsed && (DataFlags & 0x0004) > 0)
            {
                EmissionCommonRGBValue = StreamUtils.ReadFloat(stream);
            }

            else
            {
                EmissionColour = new RGBA(stream);
            }

            Shininess = StreamUtils.ReadFloat(stream);
        }
    }
}