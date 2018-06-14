using System;
using System.Collections.Generic;
using System.IO;

#if DEBUG
using System.Diagnostics;
#endif

namespace JTfy
{
    public class MetaDataSegment : BaseDataStructure
    {
#if DEBUG
        private ElementHeader metaDataElementElementHeader;
#endif

        public BaseDataStructure MetaDataElement { get; private set; }

        public override int ByteCount { get { return ElementHeader.Size + MetaDataElement.ByteCount; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                var objectTypeIdBaseTypePair = ConstUtils.TypeToObjectTypeId[MetaDataElement.GetType()];

                bytesList.AddRange(new ElementHeader(MetaDataElement.ByteCount + GUID.Size + 1, new GUID(objectTypeIdBaseTypePair.Item1), objectTypeIdBaseTypePair.Item2).Bytes);
                bytesList.AddRange(MetaDataElement.Bytes);

                return bytesList.ToArray();
            }
        }

        public MetaDataSegment(BaseDataStructure metaDataElement)
        {
            MetaDataElement = metaDataElement;
        }

        public MetaDataSegment(Stream stream)
        {
            var elementHeader = new ElementHeader(stream);

#if DEBUG
            metaDataElementElementHeader = elementHeader;
#endif

            var objectTypeIdAsString = elementHeader.ObjectTypeID.ToString();

            if (ConstUtils.ObjectTypeIdToType.ContainsKey(objectTypeIdAsString))
                MetaDataElement = (BaseDataStructure)Activator.CreateInstance(ConstUtils.ObjectTypeIdToType[objectTypeIdAsString].Item1, new object[] { stream });
            else
                throw new NotImplementedException(String.Format("Case not defined for Graph Element Object Type {0}", objectTypeIdAsString));

#if DEBUG
            Debug.Assert(stream.Length == stream.Position, "End of stream not reached at the end of MetaDataSegment");
#endif

            stream.Dispose();
        }
    }
}