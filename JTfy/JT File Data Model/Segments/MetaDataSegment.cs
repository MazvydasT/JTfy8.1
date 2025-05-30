﻿using System.Diagnostics;

namespace JTfy
{
    public class MetaDataSegment : BaseDataStructure
    {
        public BaseDataStructure MetaDataElement { get; private set; }

        public override int ByteCount { get { return ElementHeader.Size + MetaDataElement.ByteCount; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);
                var (objectTypeId, objectBaseType, _) = ConstUtils.TypeToObjectTypeId[MetaDataElement.GetType()];

                bytesList.AddRange(new ElementHeader(MetaDataElement.ByteCount + GUID.Size + 1, new GUID(objectTypeId), objectBaseType).Bytes);
                bytesList.AddRange(MetaDataElement.Bytes);

                return [.. bytesList];
            }
        }

        public MetaDataSegment(BaseDataStructure metaDataElement)
        {
            MetaDataElement = metaDataElement;
        }

        public MetaDataSegment(Stream stream)
        {
            var elementHeader = new ElementHeader(stream);

            var objectTypeIdAsString = elementHeader.ObjectTypeID.ToString();

            if (ConstUtils.ObjectTypeIdToType.TryGetValue(objectTypeIdAsString, out var value))
                MetaDataElement = (BaseDataStructure)value.factory(stream);

            else
                throw new NotImplementedException(String.Format("Case not defined for Graph Element Object Type {0}", objectTypeIdAsString));

            Debug.Assert(stream.Length == stream.Position, "End of stream not reached at the end of MetaDataSegment");

            stream.Dispose();
        }
    }
}