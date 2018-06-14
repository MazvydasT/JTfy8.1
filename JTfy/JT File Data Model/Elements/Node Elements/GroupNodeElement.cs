using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class GroupNodeElement : BaseNodeElement
    {
        public int ChildCount { get; protected set; }
        public int[] ChildNodeObjectIds { get; protected set; }

        public override int ByteCount { get { return base.ByteCount + 4 + ChildCount * 4; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(ChildCount));

                for (int i = 0; i < ChildCount; ++i)
                {
                    bytesList.AddRange(StreamUtils.ToBytes(ChildNodeObjectIds[i]));
                }

                return bytesList.ToArray();
            }
        }

        public GroupNodeElement(Stream stream)
            : base(stream)
        {
            ChildCount = StreamUtils.ReadInt32(stream);
            ChildNodeObjectIds = new int[ChildCount];

            for (int i = 0; i < ChildCount; ++i)
            {
                ChildNodeObjectIds[i] = StreamUtils.ReadInt32(stream);
            }
        }

        public GroupNodeElement(int objectId, int[] childNodeObjectIds, int[] attributeObjectIds = null)
            : base(objectId, attributeObjectIds)
        {
            if (childNodeObjectIds == null) childNodeObjectIds = new int[0];

            ChildCount = childNodeObjectIds.Length;
            ChildNodeObjectIds = childNodeObjectIds;
        }
    }
}