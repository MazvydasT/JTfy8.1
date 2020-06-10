using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class GroupNodeElement : BaseNodeElement
    {
        public int ChildCount { get { return ChildNodeObjectIds.Count; } }

        private List<int> childNodeObjectIds = new List<int>();
        public List<int> ChildNodeObjectIds { get { return childNodeObjectIds; } set { childNodeObjectIds = value ?? new List<int>(); } }

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
            var childCount = StreamUtils.ReadInt32(stream);
            ChildNodeObjectIds = new List<int>(childCount);

            for (int i = 0; i < childCount; ++i)
            {
                ChildNodeObjectIds.Add(StreamUtils.ReadInt32(stream));
            }
        }

        public GroupNodeElement(int objectId) : base(objectId) { }
    }
}