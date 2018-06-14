using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class DatePropertyAtomElement : BasePropertyAtomElement
    {
        public Date Date { get; private set; }

        public override int ByteCount
        {
            get
            {
                return base.ByteCount + Date.ByteCount;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<Byte>(ByteCount);

                bytesList.AddRange(base.Bytes);

                bytesList.AddRange(Date.Bytes);

                return bytesList.ToArray();
            }
        }

        public DatePropertyAtomElement(DateTime dateTime, int objectId) : this(new Date(dateTime), objectId) { }

        public DatePropertyAtomElement(Date date, int objectId)
            : base(objectId)
        {
            Date = date;
        }

        public DatePropertyAtomElement(Stream stream)
            : base(stream)
        {
            Date = new Date(stream);
        }
    }
}