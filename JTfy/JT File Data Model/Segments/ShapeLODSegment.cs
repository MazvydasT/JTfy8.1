using System;
using System.Collections.Generic;
using System.IO;

using System.Diagnostics;

namespace JTfy
{
    public class ShapeLODSegment : BaseDataStructure
    {
        public BaseDataStructure ShapeLODElement { get; private set; }

        public override int ByteCount
        {
            get { return ElementHeader.Size + ShapeLODElement.ByteCount; }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                var objectTypeIdBaseTypePair = ConstUtils.TypeToObjectTypeId[ShapeLODElement.GetType()];

                bytesList.AddRange(new ElementHeader(ShapeLODElement.ByteCount + GUID.Size + 1, new GUID(objectTypeIdBaseTypePair.Item1), objectTypeIdBaseTypePair.Item2).Bytes);
                bytesList.AddRange(ShapeLODElement.Bytes);

                return bytesList.ToArray();
            }
        }

        public ShapeLODSegment(BaseDataStructure shapeLODElement)
        {
            ShapeLODElement = shapeLODElement;
        }

        public ShapeLODSegment(Stream stream)
        {
            var elementHeader = new ElementHeader(stream);
            
            var objectTypeIdAsString = elementHeader.ObjectTypeID.ToString();

            if (ConstUtils.ObjectTypeIdToType.ContainsKey(objectTypeIdAsString))
                ShapeLODElement = (BaseDataStructure)Activator.CreateInstance(ConstUtils.ObjectTypeIdToType[objectTypeIdAsString].Item1, new object[] { stream });
            else
                throw new NotImplementedException(String.Format("Case not defined for Shape LOD Element Object Type {0}", objectTypeIdAsString));
            
            Debug.Assert(stream.Length == stream.Position, "End of stream not reached at the end of ShapeLODSegment");
            
            stream.Dispose();
        }
    }
}