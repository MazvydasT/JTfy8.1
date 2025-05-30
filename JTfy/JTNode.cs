﻿namespace JTfy
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
        private static readonly Dictionary<MeasurementUnits, string> measurementUnitStrings = [];

        public int ID { get; set; } = IdGenUtils.NextId;

        private Dictionary<string, object> attributes = [];
        public Dictionary<string, object> Attributes { get { return attributes; } set { attributes = value ?? []; } }

        private List<JTNode> children = [];
        public List<JTNode> Children { get { return children; } set { children = value ?? []; } }

        public MeasurementUnits MeasurementUnit { get; set; } = MeasurementUnits.Millimeters;
        public string MeasurementUnitAsString
        {
            get
            {
                if (measurementUnitStrings.TryGetValue(MeasurementUnit, out var value)) return value;

                var measurementUnitString = MeasurementUnit.ToString();

                measurementUnitStrings[MeasurementUnit] = measurementUnitString;

                return measurementUnitString;
            }
        }

        public string? Number { get; set; } = null;

        public string? Name { get; set; } = null;

        private GeometricSet[] geometricSets = [];
        public GeometricSet[] GeometricSets { get { return geometricSets; } set { geometricSets = value ?? ([]); } }

        public float[]? TransformationMatrix { get; set; }

        private readonly Dictionary<string, int> uniquePropertyIds = [];
        private readonly Dictionary<string, int> uniqueAttributeIds = [];
        private readonly Dictionary<JTNode, SegmentHeader> uniqueMetaDataSegmentHeaders = [];

        private readonly Dictionary<Int32, NodePropertyTable> propertyTableContents = [];

        private readonly List<BaseDataStructure> elements = [];
        private readonly List<BasePropertyAtomElement> propertyAtomElements = [];

        private readonly Dictionary<int, PartitionNodeElement> savedFileIds = [];

        private readonly List<ShapeLODSegment> shapeLODSegments = [];
        private readonly List<SegmentHeader> shapeLODSegmentHeaders = [];

        private readonly List<Byte[]> compressedMetaDataSegments = [];
        private readonly List<LogicElementHeaderZLIB> metaDataSegmentHeadersZLIB = [];
        private readonly List<SegmentHeader> metaDataSegmentHeaders = [];

        private bool monolithic;
        private bool separateAttributeSegments;
        private string savePath = "";

        public JTNode() { }

        public JTNode(JTNode node)
        {
            ID = node.ID;
            Attributes = node.Attributes;
            Children = node.Children;
            GeometricSets = node.GeometricSets;
            MeasurementUnit = node.MeasurementUnit;
            Number = node.Number;
            Name = node.Name;
            TransformationMatrix = node.TransformationMatrix;
        }

        public JTNode Clone() { return new JTNode(this); }

        public PartitionNodeElement Save(string path, bool monolithic = true, bool separateAttributeSegments = false, Action<float?, string, string>? onProgress = null)
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

            this.savePath = Path.Combine(String.Join("_", Path.GetDirectoryName(path)?.Split(Path.GetInvalidPathChars()) ?? []), String.Join("_", Path.GetFileName(path).Split(Path.GetInvalidFileNameChars())));

            // File Header

            var fileHeader = new FileHeader("Version 8.1 JT", (Byte)(BitConverter.IsLittleEndian ? 0 : 1), FileHeader.Size, GUID.NewGUID());

            // END File Header



            // Create all elements

            //FindInstancedNodes(this);

            CreateElement(this, onProgress);

            // END Create all elements



            // LSG Segment

            var keys = new int[propertyTableContents.Keys.Count];
            propertyTableContents.Keys.CopyTo(keys, 0);

            var values = new NodePropertyTable[propertyTableContents.Values.Count];
            propertyTableContents.Values.CopyTo(values, 0);

            var lsgSegment = new LSGSegment([.. elements], propertyAtomElements, new PropertyTable(keys, values));

            // END LSG Segment



            // Compress LSG Segment

            onProgress?.Invoke(.33f, "Compressing stuff", "");

            var compressedLSGSegmentData = CompressionUtils.Compress(lsgSegment.Bytes);

            onProgress?.Invoke(.44f, "Compressing stuff", "- done!");

            // END Compress LSG Segment


            onProgress?.Invoke(.66f, "Performing binary transmutations", "");

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

            var tocSegment = new TOCSegment([.. tocEntries]);

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

            onProgress?.Invoke(.77f, "Performing binary transmutations", "- done!");

            // END Write to file

            if (elements.Count == 0)
                throw new Exception("Writing JT file failed");

            return (PartitionNodeElement)elements[0];
        }

        private BaseNodeElement CreateElement(JTNode node, Action<float?, string, string>? onProgress = null)
        {
            if (!monolithic && node.GeometricSets.Length > 0)
            {
                PartitionNodeElement partitionElement;

                if (savedFileIds.TryGetValue(node.ID, out var value)) partitionElement = value;

                else
                {
                    var partFileName = String.Join("_", node.Number?.Split(Path.GetInvalidFileNameChars()) ?? []) + "_" + node.ID + ".jt";
                    var partFileDirectory = Path.Combine(Path.GetDirectoryName(savePath) ?? "", Path.GetFileNameWithoutExtension(savePath));
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

                ProcessAttributes(new JTNode(node) { GeometricSets = [] }, instanceElement.ObjectId);

                return instanceElement;
            }

            // Process children and store their IDs

            var childNodes = node.Children;
            var childNodesCount = childNodes.Count;
            var childNodeObjectIds = new List<int>(childNodesCount);

            for (int i = 0; i < childNodesCount; ++i)
            {
                childNodeObjectIds.Add(CreateElement(childNodes[i], onProgress).ObjectId);
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

                if (uniqueAttributeIds.TryGetValue(transformationMatrixAsString, out var value))
                    geometricTransformAttributeElementId = value;
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

                    if (uniqueAttributeIds.TryGetValue(colourAsString, out var value))
                        materialAttributeElementId = value;
                    else
                    {
                        materialAttributeElementId = IdGenUtils.NextId;
                        uniqueAttributeIds[colourAsString] = materialAttributeElementId;
                        elements.Add(new MaterialAttributeElement(colour, materialAttributeElementId));
                    }

                    var triStripSetShapeNodeElement = new TriStripSetShapeNodeElement(geometricSet, IdGenUtils.NextId)
                    {
                        AttributeObjectIds = [materialAttributeElementId]
                    };

                    elements.Add(triStripSetShapeNodeElement);

                    groupNodeElement.ChildNodeObjectIds.Add(triStripSetShapeNodeElement.ObjectId);

                    x += geometricSet.Center.X;
                    y += geometricSet.Center.Y;
                    z += geometricSet.Center.Z;
                    count++;

                    ProcessAttributes(new JTNode()
                    {
                        GeometricSets = [geometricSet]
                    }, triStripSetShapeNodeElement.ObjectId);
                }

                var rangeLODNodeElement = new RangeLODNodeElement(IdGenUtils.NextId)
                {
                    ChildNodeObjectIds = [groupNodeElement.ObjectId],

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
                    BBoxF32? untransformedBBox;

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

                    var minCorner = untransformedBBox?.MinCorner ?? new();
                    var maxCorner = untransformedBBox?.MaxCorner ?? new();

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
                    ChildNodeObjectIds = [nodeElement.ObjectId],

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

            onProgress?.Invoke(null, "Building JT structures", "");

            return nodeElement;
        }

        private void ProcessAttributes(JTNode node, int nodeElementId)
        {
            var attributes = new Dictionary<string, object>(node.Attributes.Count);

            foreach (var attribute in node.Attributes)
            {
                var key = attribute.Key.Trim();
                var value = attribute.Value;

                while (key.EndsWith(':')) key = key[..^1];
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

            if (node.Number != null || node.Name != null) attributes["JT_PROP_NAME"] = string.Join(" - ", new[] { node.Number, node.Name }.Where(v => v != null).ToArray()) + "." + (node.Children.Count > 0 ? "asm" : "part") + ";0;0:";

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

                if (uniquePropertyIds.TryGetValue(keyLookupKey, out var newKeyId))
                    keyId = newKeyId;
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

                if (uniquePropertyIds.TryGetValue(valueLookupKey, out var newValueId))
                    valueId = newValueId;
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

        private SegmentHeader? GetMetaDataSegmentHeader(JTNode node)
        {
            if (uniqueMetaDataSegmentHeaders.TryGetValue(node, out var existingMetaDataSegmentHeader)) return existingMetaDataSegmentHeader;

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