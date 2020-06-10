using System;
using System.Collections.Generic;
using System.IO;

#if DEBUG
using System.Diagnostics;
#endif

namespace JTfy
{
    public class LSGSegment : BaseDataStructure
    {
#if DEBUG
        private readonly List<ElementHeader> graphElementHeaders = new List<ElementHeader>();
        private readonly List<ElementHeader> propertyAtomElementHeaders = new List<ElementHeader>();
#endif

        public List<BaseDataStructure> GraphElements { get; private set; }
        public List<BasePropertyAtomElement> PropertyAtomElements { get; private set; }
        public PropertyTable PropertyTable { get; private set; }

        override public int ByteCount
        {
            get
            {
                var elementHeaderByteCount = ElementHeader.Size;

                var size = 0;

                for (int i = 0, c = GraphElements.Count; i < c; ++i)
                {
                    size += elementHeaderByteCount + GraphElements[i].ByteCount;
                }

                size += 4 + GUID.Size; // int length + GUID object type id

                for (int i = 0, c = PropertyAtomElements.Count; i < c; ++i)
                {
                    size += elementHeaderByteCount + PropertyAtomElements[i].ByteCount;
                }

                size += 4 + GUID.Size; // int length + GUID object type id

                size += PropertyTable.ByteCount;

                return size;
            }
        }

        public override byte[] Bytes
        {
            get
            {
                var bytesList = new List<byte>(ByteCount);

                for (int i = 0, c = GraphElements.Count; i < c; ++i)
                {
                    var graphElement = GraphElements[i];
                    var objectTypeIdBaseTypePair = ConstUtils.TypeToObjectTypeId[graphElement.GetType()];

                    bytesList.AddRange(new ElementHeader(graphElement.ByteCount + GUID.Size + 1, new GUID(objectTypeIdBaseTypePair.Item1), objectTypeIdBaseTypePair.Item2).Bytes);
                    bytesList.AddRange(graphElement.Bytes);
                }

                bytesList.AddRange(StreamUtils.ToBytes(GUID.Size));
                bytesList.AddRange(ConstUtils.EndOfElement.Bytes);

                for (int i = 0, c = PropertyAtomElements.Count; i < c; ++i)
                {
                    var propertyAtomElement = PropertyAtomElements[i];
                    var objectTypeIdBaseTypePair = ConstUtils.TypeToObjectTypeId[propertyAtomElement.GetType()];

                    bytesList.AddRange(new ElementHeader(propertyAtomElement.ByteCount + GUID.Size + 1, new GUID(objectTypeIdBaseTypePair.Item1), objectTypeIdBaseTypePair.Item2).Bytes);
                    bytesList.AddRange(propertyAtomElement.Bytes);
                }

                bytesList.AddRange(StreamUtils.ToBytes(GUID.Size));
                bytesList.AddRange(ConstUtils.EndOfElement.Bytes);

                bytesList.AddRange(PropertyTable.Bytes);

                return bytesList.ToArray();
            }
        }

        public LSGSegment(List<BaseDataStructure> graphElements, List<BasePropertyAtomElement> propertyAtomElements, PropertyTable propertyTable)
        {
            GraphElements = graphElements;
            PropertyAtomElements = propertyAtomElements;
            PropertyTable = propertyTable;
        }

        public LSGSegment(Stream stream)
        {
            GraphElements = new List<BaseDataStructure>();

            while (true)
            {
                var elementHeader = new ElementHeader(stream);

                var objectTypeIdAsString = elementHeader.ObjectTypeID.ToString();

                if (objectTypeIdAsString == ConstUtils.EndOfElementAsString) goto StartOfPropertyAtoms;

#if DEBUG
                graphElementHeaders.Add(elementHeader);
#endif

                if (ConstUtils.ObjectTypeIdToType.ContainsKey(objectTypeIdAsString))
                    GraphElements.Add((BaseDataStructure)Activator.CreateInstance(ConstUtils.ObjectTypeIdToType[objectTypeIdAsString].Item1, new object[] { stream }));
                else
                    throw new NotImplementedException(String.Format("Case not defined for Graph Element Object Type {0}", objectTypeIdAsString));
            }

        StartOfPropertyAtoms:
            PropertyAtomElements = new List<BasePropertyAtomElement>();

            while (true)
            {
                var elementHeader = new ElementHeader(stream);

                var objectTypeIdAsString = elementHeader.ObjectTypeID.ToString();

                if (objectTypeIdAsString == ConstUtils.EndOfElementAsString) goto StartOfPropertyTable;

#if DEBUG
                propertyAtomElementHeaders.Add(elementHeader);
#endif

                if (ConstUtils.ObjectTypeIdToType.ContainsKey(objectTypeIdAsString))
                    PropertyAtomElements.Add((BasePropertyAtomElement)Activator.CreateInstance(ConstUtils.ObjectTypeIdToType[objectTypeIdAsString].Item1, new object[] { stream }));
                else
                    throw new NotImplementedException(String.Format("Case not defined for Atom Property Object Type {0}", objectTypeIdAsString));
            }

        StartOfPropertyTable:

            PropertyTable = new PropertyTable(stream);

#if DEBUG
            Debug.Assert(stream.Length == stream.Position, "End of stream not reached at the end of MetaDataSegment");
#endif

            stream.Dispose();
        }
    }
}