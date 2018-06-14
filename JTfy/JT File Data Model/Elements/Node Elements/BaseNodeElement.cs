using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class BaseNodeElement : BaseDataStructure
    {
        public int ObjectId { get; protected set; }
        public uint NodeFlags { get; protected set; }
        public int AttributeCount { get; protected set; }
        public int[] AttributeObjectIds { get; protected set; }

        public override int ByteCount { get { return 4 + 4 + 4 + AttributeCount * 4; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(StreamUtils.ToBytes(ObjectId));
                bytesList.AddRange(StreamUtils.ToBytes(NodeFlags));
                bytesList.AddRange(StreamUtils.ToBytes(AttributeCount));

                for (int i = 0; i < AttributeCount; ++i)
                {
                    bytesList.AddRange(StreamUtils.ToBytes(AttributeObjectIds[i]));
                }

                return bytesList.ToArray();
            }
        }

        public BaseNodeElement(int objectId) : this(objectId, null) { }

        public BaseNodeElement(int objectId, int[] attributeObjectIds)
        {
            if (attributeObjectIds == null) attributeObjectIds = new int[0];

            ObjectId = objectId;
            NodeFlags = 0;
            AttributeCount = attributeObjectIds.Length;
            AttributeObjectIds = attributeObjectIds;
        }

        public BaseNodeElement(Stream stream)
        {
            ObjectId = StreamUtils.ReadInt32(stream);
            NodeFlags = StreamUtils.ReadUInt32(stream);
            AttributeCount = StreamUtils.ReadInt32(stream);
            AttributeObjectIds = new int[AttributeCount];

            for (int i = 0; i < AttributeCount; ++i)
            {
                AttributeObjectIds[i] = StreamUtils.ReadInt32(stream);
            }
        }
    }
}