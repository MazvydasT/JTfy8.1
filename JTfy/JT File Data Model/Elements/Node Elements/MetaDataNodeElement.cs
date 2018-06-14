﻿using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class MetaDataNodeElement : GroupNodeElement
    {
        private Int16 versionNumber = 1;

        public override int ByteCount { get { return base.ByteCount + 2; } }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                bytesList.AddRange(base.Bytes);
                bytesList.AddRange(StreamUtils.ToBytes(versionNumber));

                return bytesList.ToArray();
            }
        }

        public MetaDataNodeElement(int objectId, int[] childNodeObjectIds = null, int[] attributeObjectIds = null) : base(objectId, childNodeObjectIds, attributeObjectIds) { }

        public MetaDataNodeElement(Stream stream):base(stream)
        {
            versionNumber = StreamUtils.ReadInt16(stream);
        }
    }
}