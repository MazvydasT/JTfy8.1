using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class JTNode
    {
        public enum MeasurementUnits
        {
            Millimeters,
            Centimeters,
            Meters,
            Inches,
            Feet,
            Yards,
            Micrometers,
            Decimeters,
            Kilometers,
            Mils,
            Miles
        }
        private static readonly Dictionary<MeasurementUnits, string> measurementUnitStrings = new Dictionary<MeasurementUnits, string>();

        private int id = IdGenUtils.NextId;
        public int ID { get { return id; } set { id = value; } }

        private Dictionary<string, object> attributes = new Dictionary<string, object>();
        public Dictionary<string, object> Attributes { get { return attributes; } set { attributes = value ?? new Dictionary<string, object>(); } }

        private List<JTNode> children = new List<JTNode>();
        public List<JTNode> Children { get { return children; } set { children = value ?? new List<JTNode>(); } }

        private MeasurementUnits measurementUnit = MeasurementUnits.Millimeters;
        public MeasurementUnits MeasurementUnit { get { return measurementUnit; } set { measurementUnit = value; } }
        public string MeasurementUnitAsString
        {
            get
            {
                if (measurementUnitStrings.ContainsKey(measurementUnit)) return measurementUnitStrings[measurementUnit];

                var measurementUnitString = MeasurementUnit.ToString();

                measurementUnitStrings[measurementUnit] = measurementUnitString;

                return measurementUnitString;
            }
        }

        private string name = null;
        public string Name { get { return name; } set { name = value; } }

        private GeometricSet[] geometricSets = new GeometricSet[0];
        public GeometricSet[] GeometricSets { get { return geometricSets; } set { geometricSets = value ?? (new GeometricSet[0]); } }

        public float[] TransformationMatrix { get; set; }

        private readonly Dictionary<string, int> uniquePropertyIds = new Dictionary<string, int>();
        private readonly Dictionary<string, int> uniqueAttributeIds = new Dictionary<string, int>();
        private readonly Dictionary<JTNode, SegmentHeader> uniqueMetaDataSegmentHeaders = new Dictionary<JTNode, SegmentHeader>();

        private readonly Dictionary<Int32, NodePropertyTable> propertyTableContents = new Dictionary<Int32, NodePropertyTable>();

        private readonly List<BaseDataStructure> elements = new List<BaseDataStructure>();
        private readonly List<BasePropertyAtomElement> propertyAtomElements = new List<BasePropertyAtomElement>();

        private readonly Dictionary<int, PartitionNodeElement> savedFileIds = new Dictionary<int, PartitionNodeElement>();

        private readonly List<ShapeLODSegment> shapeLODSegments = new List<ShapeLODSegment>();
        private readonly List<SegmentHeader> shapeLODSegmentHeaders = new List<SegmentHeader>();

        private readonly List<Byte[]> compressedMetaDataSegments = new List<Byte[]>();
        private readonly List<LogicElementHeaderZLIB> metaDataSegmentHeadersZLIB = new List<LogicElementHeaderZLIB>();
        private readonly List<SegmentHeader> metaDataSegmentHeaders = new List<SegmentHeader>();

        private bool monolithic;
        private bool separateAttributeSegments;
        private string savePath;

        public JTNode() { }

        public JTNode(JTNode node)
        {
            ID = node.ID;
            Attributes = node.Attributes;
            Children = node.Children;
            GeometricSets = node.GeometricSets;
            MeasurementUnit = node.MeasurementUnit;
            Name = node.Name;
            TransformationMatrix = node.TransformationMatrix;
        }

        public JTNode Clone() { return new JTNode(this); }

        public PartitionNodeElement Save(string path, bool monolithic = true, bool separateAttributeSegments = false)
        {
            uniquePropertyIds.Clear();
            uniqueAttributeIds.Clear();
            uniqueMetaDataSegmentHeaders.Clear();

            propertyTableContents.Clear();

            elements.Clear();
            propertyAtomElements.Clear();

            savedFileIds.Clear();

            shapeLODSegments.Clear();
            shapeLODSegmentHeaders.Clear();

            compressedMetaDataSegments.Clear();
            metaDataSegmentHeadersZLIB.Clear();
            metaDataSegmentHeaders.Clear();

            this.monolithic = monolithic;
            this.separateAttributeSegments = separateAttributeSegments;

            this.savePath = Path.Combine(String.Join("_", Path.GetDirectoryName(path).Split(Path.GetInvalidPathChars())), String.Join("_", Path.GetFileName(path).Split(Path.GetInvalidFileNameChars())));

            // File Header

            var fileHeader = new FileHeader("Version 8.1 JT", (Byte)(BitConverter.IsLittleEndian ? 0 : 1), FileHeader.Size, GUID.NewGUID());

            // END File Header



            // Create all elements

            //FindInstancedNodes(this);

            CreateElement(this);

            // END Create all elements



            // LSG Segment

            var keys = new int[propertyTableContents.Keys.Count];
            propertyTableContents.Keys.CopyTo(keys, 0);

            var values = new NodePropertyTable[propertyTableContents.Values.Count];
            propertyTableContents.Values.CopyTo(values, 0);

            var lsgSegment = new LSGSegment(new List<BaseDataStructure>(elements), propertyAtomElements, new PropertyTable(keys, values));

            // END LSG Segment



            // Compress LSG Segment

            var compressedLSGSegmentData = CompressionUtils.Compress(lsgSegment.Bytes);

            // END Compress LSG Segment



            // LSG Segment Logic Element Header ZLIB

            var lsgSegmentLogicElementHeaderZLIB = new LogicElementHeaderZLIB(2, compressedLSGSegmentData.Length + 1, 2); // CompressionAlgorithm field (of type Byte) is included in CompressedDataLength

            // END LSG Segment Logic Element Header ZLIB



            // Segment Header

            var lsgSegmentHeader = new SegmentHeader(fileHeader.LSGSegmentID, 1, SegmentHeader.Size + lsgSegmentLogicElementHeaderZLIB.ByteCount + compressedLSGSegmentData.Length);

            // END Segment Header



            // Toc Segments

            var lsgTOCEntry = new TOCEntry(lsgSegmentHeader.SegmentID, -1, lsgSegmentHeader.SegmentLength, (UInt32)(lsgSegmentHeader.SegmentType << 24));

            var tocEntries = new List<TOCEntry>()
            {
                lsgTOCEntry
            };

            for (int i = 0, c = shapeLODSegmentHeaders.Count; i < c; ++i)
            {
                var shapeLODSegmentHeader = shapeLODSegmentHeaders[i];

                tocEntries.Add(new TOCEntry(shapeLODSegmentHeader.SegmentID, -1, shapeLODSegmentHeader.SegmentLength, (UInt32)(shapeLODSegmentHeader.SegmentType << 24)));
            }

            for (int i = 0, c = metaDataSegmentHeaders.Count; i < c; ++i)
            {
                var metaDataSegmentHeader = metaDataSegmentHeaders[i];

                tocEntries.Add(new TOCEntry(metaDataSegmentHeader.SegmentID, -1, metaDataSegmentHeader.SegmentLength, (UInt32)(metaDataSegmentHeader.SegmentType << 24)));
            }

            if (tocEntries.Count == 1) tocEntries.Add(lsgTOCEntry);

            var tocSegment = new TOCSegment(tocEntries.ToArray());

            var segmentTotalSizeTracker = 0;

            for (int i = 0, c = tocEntries.Count; i < c; ++i)
            {
                var tocEntry = tocEntries[i];

                if (i > 0 && tocEntry == tocEntries[i - 1]) continue;

                tocEntry.SegmentOffset = fileHeader.ByteCount + tocSegment.ByteCount + segmentTotalSizeTracker;

                segmentTotalSizeTracker += tocEntry.SegmentLength;
            }

            // END Toc Segments


            // Write to file

            using (var outputFileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            {
                outputFileStream.Write(fileHeader.Bytes, 0, fileHeader.ByteCount);

                outputFileStream.Write(tocSegment.Bytes, 0, tocSegment.ByteCount);

                outputFileStream.Write(lsgSegmentHeader.Bytes, 0, lsgSegmentHeader.ByteCount);

                outputFileStream.Write(lsgSegmentLogicElementHeaderZLIB.Bytes, 0, lsgSegmentLogicElementHeaderZLIB.ByteCount);

                outputFileStream.Write(compressedLSGSegmentData, 0, compressedLSGSegmentData.Length);

                for (int i = 0, c = shapeLODSegmentHeaders.Count; i < c; ++i)
                {
                    var shapeLODSegmentHeader = shapeLODSegmentHeaders[i];
                    var shapeLODSegment = shapeLODSegments[i];

                    outputFileStream.Write(shapeLODSegmentHeader.Bytes, 0, shapeLODSegmentHeader.ByteCount);
                    outputFileStream.Write(shapeLODSegment.Bytes, 0, shapeLODSegment.ByteCount);
                }

                for (int i = 0, c = metaDataSegmentHeaders.Count; i < c; ++i)
                {
                    var metaDataSegmentHeader = metaDataSegmentHeaders[i];
                    var metaDataSegmentHeaderZLIB = metaDataSegmentHeadersZLIB[i];
                    var compressedMetaDataSegment = compressedMetaDataSegments[i];

                    outputFileStream.Write(metaDataSegmentHeader.Bytes, 0, metaDataSegmentHeader.ByteCount);
                    outputFileStream.Write(metaDataSegmentHeaderZLIB.Bytes, 0, metaDataSegmentHeaderZLIB.ByteCount);
                    outputFileStream.Write(compressedMetaDataSegment, 0, compressedMetaDataSegment.Length);
                }
            }

            // END Write to file

            return elements.Count > 0 ? (PartitionNodeElement)elements[0] : null;
        }

        private BaseNodeElement CreateElement(JTNode node)
        {
            if (!monolithic && node.GeometricSets.Length > 0)
            {
                PartitionNodeElement partitionElement;

                if (savedFileIds.ContainsKey(node.ID)) partitionElement = savedFileIds[node.ID];

                else
                {
                    var partFileName = String.Join("_", node.Name.Split(Path.GetInvalidFileNameChars())) + "_" + node.ID + ".jt";
                    var partFileDirectory = Path.Combine(Path.GetDirectoryName(savePath), Path.GetFileNameWithoutExtension(savePath));
                    var partFilePath = Path.Combine(partFileDirectory, partFileName);

                    if (!Directory.Exists(partFileDirectory)) Directory.CreateDirectory(partFileDirectory);

                    partitionElement = new PartitionNodeElement(IdGenUtils.NextId, new JTNode(node) { TransformationMatrix = null }.Save(partFilePath, true, this.separateAttributeSegments))
                    {
                        FileName = new MbString(@".\" + Path.GetFileNameWithoutExtension(savePath) + @"\" + partFileName)
                    };

                    elements.Add(partitionElement);

                    savedFileIds[node.ID] = partitionElement;
                }

                var instanceElement = new InstanceNodeElement(partitionElement.ObjectId, IdGenUtils.NextId);

                elements.Add(instanceElement);

                ProcessAttributes(new JTNode(node) { GeometricSets = null }, instanceElement.ObjectId);

                return instanceElement;
            }

            // Process children and store their IDs

            var childNodes = node.Children;
            var childNodesCount = childNodes.Count;
            var childNodeObjectIds = new List<int>(childNodesCount);

            for (int i = 0; i < childNodesCount; ++i)
            {
                childNodeObjectIds.Add(CreateElement(childNodes[i]).ObjectId);
            }

            // END Process children and store their IDs



            // Create node

            MetaDataNodeElement nodeElement = node.GeometricSets.Length > 0 ? new PartNodeElement(IdGenUtils.NextId) : new MetaDataNodeElement(IdGenUtils.NextId);

            nodeElement.ChildNodeObjectIds = childNodeObjectIds;

            // END Create node



            // Process transformatio matrix

            if (node.TransformationMatrix != null)
            {
                var transformationMatrixAsString = String.Join("|", node.TransformationMatrix);

                int geometricTransformAttributeElementId;

                if (uniqueAttributeIds.ContainsKey(transformationMatrixAsString))
                    geometricTransformAttributeElementId = uniqueAttributeIds[transformationMatrixAsString];
                else
                {
                    geometricTransformAttributeElementId = IdGenUtils.NextId;
                    uniqueAttributeIds[transformationMatrixAsString] = geometricTransformAttributeElementId;
                    elements.Add(new GeometricTransformAttributeElement(node.TransformationMatrix, geometricTransformAttributeElementId));
                }

                nodeElement.AttributeObjectIds.Add(geometricTransformAttributeElementId);
            }

            // END Process transformatio matrix



            // Process Geometric Sets

            var geometricSetsCount = node.geometricSets.Length;

            if (geometricSetsCount > 0)
            {
                float x = 0, y = 0, z = 0;
                int count = 0;

                var groupNodeElement = new GroupNodeElement(IdGenUtils.NextId);
                elements.Add(groupNodeElement);

                for (int i = 0; i < geometricSetsCount; ++i)
                {
                    var geometricSet = node.GeometricSets[i];
                    var colour = geometricSet.Colour;
                    var colourAsString = colour.ToString();

                    int materialAttributeElementId;

                    if (uniqueAttributeIds.ContainsKey(colourAsString))
                        materialAttributeElementId = uniqueAttributeIds[colourAsString];
                    else
                    {
                        materialAttributeElementId = IdGenUtils.NextId;
                        uniqueAttributeIds[colourAsString] = materialAttributeElementId;
                        elements.Add(new MaterialAttributeElement(colour, materialAttributeElementId));
                    }

                    var triStripSetShapeNodeElement = new TriStripSetShapeNodeElement(geometricSet, IdGenUtils.NextId)
                    {
                        AttributeObjectIds = new List<int>() { materialAttributeElementId }
                    };

                    elements.Add(triStripSetShapeNodeElement);

                    groupNodeElement.ChildNodeObjectIds.Add(triStripSetShapeNodeElement.ObjectId);

                    x += geometricSet.Center.X;
                    y += geometricSet.Center.Y;
                    z += geometricSet.Center.Z;
                    count++;

                    ProcessAttributes(new JTNode()
                    {
                        GeometricSets = new GeometricSet[] { geometricSet }
                    }, triStripSetShapeNodeElement.ObjectId);
                }

                var rangeLODNodeElement = new RangeLODNodeElement(IdGenUtils.NextId)
                {
                    ChildNodeObjectIds = new List<int>() { groupNodeElement.ObjectId },

                    Center = new CoordF32(x / count, y / count, z / count)
                };

                elements.Add(rangeLODNodeElement);

                nodeElement.ChildNodeObjectIds.Add(rangeLODNodeElement.ObjectId);
            }

            // END Process Geometric Sets



            // Process root element

            if (node == this)
            {
                float area = 0;

                int vertexCountMin = 0,
                    vertexCountMax = 0,

                    nodeCountMin = 0,
                    nodeCountMax = 0,

                    polygonCountMin = 0,
                    polygonCountMax = 0;

                float
                    minX = 0, minY = 0, minZ = 0,
                    maxX = 0, maxY = 0, maxZ = 0;

                bool firstTriStripSetShapeNodeElementVisited = false;

                var triStripSetShapeNodeElementType = typeof(TriStripSetShapeNodeElement);
                var partitionNodeElementType = typeof(PartitionNodeElement);

                for (int elementIndex = 0, elementCount = elements.Count; elementIndex < elementCount; ++elementIndex)
                {
                    var element = elements[elementIndex];
                    var elementType = element.GetType();

                    if ((monolithic && elementType != triStripSetShapeNodeElementType) || (!monolithic && elementType != partitionNodeElementType)) continue;

                    CountRange vertexCountRange, nodeCountRange, polygonCountRange;
                    BBoxF32 untransformedBBox;

                    if (monolithic)
                    {
                        var triStripSetShapeNodeElement = (TriStripSetShapeNodeElement)element;

                        area += triStripSetShapeNodeElement.Area;

                        vertexCountRange = triStripSetShapeNodeElement.VertexCountRange;
                        nodeCountRange = triStripSetShapeNodeElement.NodeCountRange;
                        polygonCountRange = triStripSetShapeNodeElement.PolygonCountRange;

                        untransformedBBox = triStripSetShapeNodeElement.UntransformedBBox;
                    }

                    else
                    {
                        var childPartitionNodeElement = (PartitionNodeElement)element;

                        area += childPartitionNodeElement.Area;

                        vertexCountRange = childPartitionNodeElement.VertexCountRange;
                        nodeCountRange = childPartitionNodeElement.NodeCountRange;
                        polygonCountRange = childPartitionNodeElement.PolygonCountRange;

                        untransformedBBox = childPartitionNodeElement.UntransformedBBox;
                    }

                    vertexCountMin += vertexCountRange.Min;
                    vertexCountMax += vertexCountRange.Max;

                    nodeCountMin += nodeCountRange.Min;
                    nodeCountMax += nodeCountRange.Max;

                    polygonCountMin += polygonCountRange.Min;
                    polygonCountMax += polygonCountRange.Max;

                    var minCorner = untransformedBBox.MinCorner;
                    var maxCorner = untransformedBBox.MaxCorner;

                    if (!firstTriStripSetShapeNodeElementVisited)
                    {
                        minX = minCorner.X;
                        minY = minCorner.Y;
                        minZ = minCorner.Z;

                        maxX = maxCorner.X;
                        maxY = maxCorner.Y;
                        maxZ = maxCorner.Z;

                        firstTriStripSetShapeNodeElementVisited = true;
                    }

                    else
                    {
                        if (minCorner.X < minX) minX = minCorner.X;
                        if (minCorner.Y < minY) minY = minCorner.Y;
                        if (minCorner.Z < minZ) minZ = minCorner.Z;

                        if (maxCorner.X > maxX) maxX = maxCorner.X;
                        if (maxCorner.Y > maxY) maxY = maxCorner.Y;
                        if (maxCorner.Z > maxZ) maxZ = maxCorner.Z;
                    }
                }

                var partitionNodeElement = new PartitionNodeElement(IdGenUtils.NextId)
                {
                    ChildNodeObjectIds = new List<int>() { nodeElement.ObjectId },

                    Area = area,
                    VertexCountRange = new CountRange(vertexCountMin, vertexCountMax),
                    NodeCountRange = new CountRange(nodeCountMin, nodeCountMax),
                    PolygonCountRange = new CountRange(polygonCountMin, polygonCountMax),
                    UntransformedBBox = new BBoxF32(minX, minY, minZ, maxX, maxY, maxZ)
                };

                elements.Insert(0, partitionNodeElement);

                ProcessAttributes(node, partitionNodeElement.ObjectId);
            }

            // END Process root element

            elements.Add(nodeElement);

            ProcessAttributes(node, nodeElement.ObjectId);

            return nodeElement;
        }

        private void ProcessAttributes(JTNode node, int nodeElementId)
        {
            var attributes = new Dictionary<string, object>(node.Attributes.Count);

            foreach (var attribute in node.Attributes)
            {
                var key = attribute.Key.Trim();
                var value = attribute.Value;

                while (key.EndsWith(":")) key = key.Substring(0, key.Length - 1);
                while (key.Contains("::")) key = key.Replace("::", ":");

                if (key.Length == 0) continue;

                attributes[key + "::"] = value;
            }

            if (separateAttributeSegments)
            {
                var metaDataSegmentHeader = GetMetaDataSegmentHeader(new JTNode() { Attributes = attributes });

                attributes.Clear();

                if (metaDataSegmentHeader != null)
                {
                    attributes["JT_LLPROP_METADATA"] = metaDataSegmentHeader;
                }
            }

            attributes["JT_PROP_MEASUREMENT_UNITS"] = node.MeasurementUnitAsString;

            if (node.Name != null) attributes["JT_PROP_NAME"] = node.Name + "." + (node.Children.Count > 0 ? "asm" : "part") + ";0;0:";

            //if (node == this && node.Children.Count > 0) attributes["PartitionType"] = "Assembly";

            if (node.GeometricSets.Length > 0) attributes["JT_LLPROP_SHAPEIMPL"] = node.GeometricSets;

            var attributesCount = attributes.Count;

            var keys = new List<int>(attributesCount);
            var values = new List<int>(attributesCount);

            foreach (var attribute in attributes)
            {
                var key = attribute.Key;

                var value = attribute.Value;
                var valueTypeName = value.GetType().Name;

                if (valueTypeName != "String" && valueTypeName != "Int32" && valueTypeName != "Single" && valueTypeName != "DateTime" && valueTypeName != "GeometricSet[]" && valueTypeName != "SegmentHeader")
                {
                    throw new Exception(String.Format("Only String, Int32, Single, DateTime, GeometricSet[] and SegmentHeader value types are allowed. Current value is {0}.", valueTypeName));
                }

                var keyLookupKey = String.Format("{0}-{1}", key.GetType().Name, key);

                int keyId;

                if (uniquePropertyIds.ContainsKey(keyLookupKey))
                    keyId = uniquePropertyIds[keyLookupKey];
                else
                {
                    keyId = IdGenUtils.NextId;
                    propertyAtomElements.Add(new StringPropertyAtomElement(key, keyId));
                    uniquePropertyIds[keyLookupKey] = keyId;
                }

                keys.Add(keyId);

                var valueAsString = valueTypeName != "GeometricSet[]" ? value.ToString() : ((GeometricSet[])(value))[0].ToString();
                var valueLookupKey = valueTypeName + "-" + valueAsString;

                int valueId;

                if (uniquePropertyIds.ContainsKey(valueLookupKey))
                    valueId = uniquePropertyIds[valueLookupKey];
                else
                {
                    valueId = IdGenUtils.NextId;
                    uniquePropertyIds[valueLookupKey] = valueId;

                    switch (valueTypeName)
                    {
                        case "String": propertyAtomElements.Add(new StringPropertyAtomElement((string)value, valueId)); break;
                        case "Int32": propertyAtomElements.Add(new IntegerPropertyAtomElement((int)value, valueId)); break;
                        case "Single": propertyAtomElements.Add(new FloatingPointPropertyAtomElement((float)value, valueId)); break;
                        case "DateTime": propertyAtomElements.Add(new DatePropertyAtomElement((DateTime)value, valueId)); break;
                        case "GeometricSet[]":
                            var geometricSet = ((GeometricSet[])value)[0];

                            var shapeLODSegment = new ShapeLODSegment(new TriStripSetShapeLODElement(geometricSet.TriStrips, geometricSet.Positions, geometricSet.Normals));
                            var shapeLODSegmentHeader = new SegmentHeader(GUID.NewGUID(), 6, SegmentHeader.Size + shapeLODSegment.ByteCount);

                            shapeLODSegments.Add(shapeLODSegment);
                            shapeLODSegmentHeaders.Add(shapeLODSegmentHeader);

                            propertyAtomElements.Add(new LateLoadedPropertyAtomElement(shapeLODSegmentHeader.SegmentID, shapeLODSegmentHeader.SegmentType, valueId));

                            break;

                        case "SegmentHeader":
                            var segmentHeader = (SegmentHeader)value;

                            propertyAtomElements.Add(new LateLoadedPropertyAtomElement(segmentHeader.SegmentID, segmentHeader.SegmentType, valueId));

                            break;
                    }
                }

                values.Add(valueId);
            }

            propertyTableContents.Add(nodeElementId, new NodePropertyTable(keys, values));
        }

        private SegmentHeader GetMetaDataSegmentHeader(JTNode node)
        {
            if (uniqueMetaDataSegmentHeaders.ContainsKey(node)) return uniqueMetaDataSegmentHeaders[node];

            var attributes = node.Attributes;

            if (attributes.Count == 0) return null;

            var keys = new List<string>(attributes.Keys);
            var values = new List<object>(attributes.Values);

            var metaDataSegment = new MetaDataSegment(new PropertyProxyMetaDataElement(keys, values));
            var compressedMetaDataSegment = CompressionUtils.Compress(metaDataSegment.Bytes);
            var metaDataSegmentHeaderZLIB = new LogicElementHeaderZLIB(2, compressedMetaDataSegment.Length + 1, 2); // CompressionAlgorithm field (of type Byte) is included in CompressedDataLength
            var metaDataSegmentHeader = new SegmentHeader(GUID.NewGUID(), 4, SegmentHeader.Size + metaDataSegmentHeaderZLIB.ByteCount + compressedMetaDataSegment.Length);

            uniqueMetaDataSegmentHeaders[node] = metaDataSegmentHeader;

            compressedMetaDataSegments.Add(compressedMetaDataSegment);
            metaDataSegmentHeadersZLIB.Add(metaDataSegmentHeaderZLIB);
            metaDataSegmentHeaders.Add(metaDataSegmentHeader);

            return metaDataSegmentHeader;
        }
    }
}