using System;
using System.Collections.Generic;
using System.IO;

namespace JTfy
{
    public class JTNode
    {
        public enum MeasurementUnits
        {
            millimeters,
            centimeters,
            meters,
            inches,
            feet,
            yards,
            micrometers,
            decimeters,
            kilometers,
            mils,
            miles
        }

        private Dictionary<string, object> attributes = new Dictionary<string, object>();
        public Dictionary<string, object> Attributes { get { return attributes; } set { attributes = value == null ? new Dictionary<string, object>() : value; } }

        private List<JTNode> children = new List<JTNode>();
        public List<JTNode> Children { get { return children; } set { children = value == null ? new List<JTNode>() : value; } }

        private MeasurementUnits measurementUnit = MeasurementUnits.millimeters;
        public MeasurementUnits MeasurementUnit { get { return measurementUnit; } set { measurementUnit = value; } }

        private string name = null;
        public string Name { get { return name; } set { name = value; } }

        private GeometricSet[] geometricSets = new GeometricSet[0];
        public GeometricSet[] GeometricSets { get { return geometricSets; } set { geometricSets = value == null ? new GeometricSet[0] : value; } }

        public float[] TransformationMatrix { get; set; }

        private HashSet<JTNode> uniqueNodes = new HashSet<JTNode>();
        private Dictionary<JTNode, BaseNodeElement> instancedNodes = new Dictionary<JTNode, BaseNodeElement>();

        private Dictionary<string, int> uniquePropertyIds = new Dictionary<string, int>();
        private Dictionary<string, int> uniqueAttributeIds = new Dictionary<string, int>();

        private Dictionary<Int32, NodePropertyTable> propertyTableContents = new Dictionary<Int32, NodePropertyTable>();

        private Stack<BaseDataStructure> elements = new Stack<BaseDataStructure>();
        private List<BasePropertyAtomElement> propertyAtomElements = new List<BasePropertyAtomElement>();
        
        private List<ShapeLODSegment> shapeLODSegments = new List<ShapeLODSegment>();
        private List<SegmentHeader> shapeLODSegmentHeaders = new List<SegmentHeader>();

        public void Save(string path)
        {
            uniqueNodes.Clear();
            instancedNodes.Clear();

            uniquePropertyIds.Clear();
            uniqueAttributeIds.Clear();

            propertyTableContents.Clear();
            
            elements.Clear();
            propertyAtomElements.Clear();

            shapeLODSegments.Clear();
            shapeLODSegmentHeaders.Clear();

            // File Header

            var fileHeader = new FileHeader("Version 8.1 JT", (Byte)(BitConverter.IsLittleEndian ? 0 : 1), FileHeader.Size, GUID.NewGUID());

            // END File Header



            // Create all elements

            FindInstancedNodes(this);

            CreateGraphElement(this);

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

            using (var outputFileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
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
            }

            // END Write to file
        }

        private void FindInstancedNodes(JTNode node)
        {
            if (uniqueNodes.Contains(node))
            {
                if (!instancedNodes.ContainsKey(node)) instancedNodes.Add(node, null);
            }

            else uniqueNodes.Add(node);

            var children = node.Children;

            for (int i = 0, c = children.Count; i < c; ++i)
            {
                FindInstancedNodes(children[i]);
            }
        }

        private BaseNodeElement CreateGraphElement(JTNode node)
        {
            var nodeElementId = IdGenUtils.NextId;
            BaseNodeElement nodeElement = null;

            // Process children and store their IDs

            var childNodes = node.Children;
            var childNodesCount = childNodes.Count;
            var childNodeObjectIds = new int[childNodesCount];

            for (int i = 0; i < childNodesCount; ++i)
            {
                childNodeObjectIds[i] = CreateGraphElement(childNodes[i]).ObjectId;
            }

            // END Process children and store their IDs

            // Process attributes
            
            var attributeObjectIdList = new List<int>();

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
                }

                elements.Push(new GeometricTransformAttributeElement(node.TransformationMatrix, geometricTransformAttributeElementId));

                attributeObjectIdList.Add(geometricTransformAttributeElementId);
            }

            var geometricSetsCount = node.geometricSets.Length;

            if (geometricSetsCount > 0)
            {
                var childNodeObjectIdList = new List<int>(childNodesCount + 1);
                childNodeObjectIdList.AddRange(childNodeObjectIds);

                var triStripSetShapeNodeElementIds = new int[geometricSetsCount];

                float x = 0, y = 0, z = 0;
                int count = 0;

                for (int i = 0; i < geometricSetsCount; ++i)
                {
                    var geometricSet = node.GeometricSets[i];

                    var materialAttributeElementId = IdGenUtils.NextId;
                    var materialAttributeElement = new MaterialAttributeElement(geometricSet.Colour, materialAttributeElementId);
                    var materialAttributeElementAsString = materialAttributeElement.ToString();

                    if (uniqueAttributeIds.ContainsKey(materialAttributeElementAsString))
                        materialAttributeElementId = uniqueAttributeIds[materialAttributeElementAsString];
                    else
                    {
                        uniqueAttributeIds[materialAttributeElementAsString] = materialAttributeElementId;
                        elements.Push(materialAttributeElement);
                    }

                    var triStripSetShapeNodeElementId = IdGenUtils.NextId;
                    elements.Push(new TriStripSetShapeNodeElement(geometricSet, triStripSetShapeNodeElementId, new int[] { materialAttributeElementId }));

                    triStripSetShapeNodeElementIds[i] = triStripSetShapeNodeElementId;

                    x += geometricSet.Center.X;
                    y += geometricSet.Center.Y;
                    z += geometricSet.Center.Z;
                    count++;

                    ProcessAttributes(new JTNode()
                    {
                        GeometricSets = new GeometricSet[]
                        {
                            geometricSet
                        }
                    }, triStripSetShapeNodeElementId);
                }

                var groupNodeElementId = IdGenUtils.NextId;
                elements.Push(new GroupNodeElement(groupNodeElementId, triStripSetShapeNodeElementIds));

                var rangeLODNodeElementId = IdGenUtils.NextId;
                elements.Push(new RangeLODNodeElement(rangeLODNodeElementId, new int[] { groupNodeElementId })
                {
                    Center = new CoordF32(x / count, y / count, z / count)
                });

                childNodeObjectIdList.Add(rangeLODNodeElementId);

                childNodeObjectIds = childNodeObjectIdList.ToArray();

                node.GeometricSets = null;

                nodeElement = new PartNodeElement(nodeElementId, childNodeObjectIds, attributeObjectIdList.ToArray());
            }

            // END Process attributes

            if (nodeElement == null)
            {
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

                    var triStripSetShapeNodeElementType = typeof(TriStripSetShapeNodeElement);

                    foreach(var element in elements)
                    {
                        if(element.GetType() != triStripSetShapeNodeElementType) continue;
                        
                        var triStripSetShapeNodeElement = (TriStripSetShapeNodeElement)element;

                            area += triStripSetShapeNodeElement.Area;

                            vertexCountMin += triStripSetShapeNodeElement.VertexCountRange.Min;
                            vertexCountMax += triStripSetShapeNodeElement.VertexCountRange.Max;

                            nodeCountMin += triStripSetShapeNodeElement.NodeCountRange.Min;
                            nodeCountMax += triStripSetShapeNodeElement.NodeCountRange.Max;

                            polygonCountMin += triStripSetShapeNodeElement.PolygonCountRange.Min;
                            polygonCountMax += triStripSetShapeNodeElement.PolygonCountRange.Max;

                            var untransformedBBox = triStripSetShapeNodeElement.UntransformedBBox;
                            var minCorner = untransformedBBox.MinCorner;
                            var maxCorner = untransformedBBox.MaxCorner;

                            if (minCorner.X < minX) minX = minCorner.X;
                            if (minCorner.Y < minY) minY = minCorner.Y;
                            if (minCorner.Z < minZ) minZ = minCorner.Z;

                            if (maxCorner.X > maxX) maxX = maxCorner.X;
                            if (maxCorner.Y > maxY) maxY = maxCorner.Y;
                            if (maxCorner.Z > maxZ) maxZ = maxCorner.Z;
                    }

                    nodeElement = new PartitionNodeElement(nodeElementId, childNodeObjectIds, attributeObjectIdList.ToArray())
                    {
                        Area = area,
                        VertexCountRange = new CountRange(vertexCountMin, vertexCountMax),
                        NodeCountRange = new CountRange(nodeCountMin, nodeCountMax),
                        PolygonCountRange = new CountRange(polygonCountMin, polygonCountMax),
                        UntransformedBBox = new BBoxF32(minX, minY, minZ, maxX, maxY, maxZ)
                    };
                }

                else
                {
                    nodeElement = childNodesCount > 0 ?
                        new GroupNodeElement(nodeElementId, childNodeObjectIds, attributeObjectIdList.ToArray()) :
                        new MetaDataNodeElement(nodeElementId, null, attributeObjectIdList.ToArray());
                }
            }

            if (instancedNodes.ContainsKey(node))
            {
                var instancedNode = instancedNodes[node];

                if (instancedNode == null)
                {
                    instancedNode = nodeElement;
                    instancedNodes[node] = instancedNode;

                    elements.Push(instancedNode);

                    //ProcessAttributes(node, instancedNode.ObjectId);
                }

                var instanceNodeElementId = IdGenUtils.NextId;
                nodeElement = new InstanceNodeElement(instancedNode.ObjectId, instanceNodeElementId);

                ProcessAttributes(node, instanceNodeElementId);

                elements.Push(nodeElement);
            }

            else
            {
                elements.Push(nodeElement);

                ProcessAttributes(node, nodeElementId);
            }

            return nodeElement;
        }

        private void ProcessAttributes(JTNode node, int nodeElementId)
        {
            node.Attributes["JT_PROP_MEASUREMENT_UNITS"] = node.MeasurementUnit.ToString();

            if (node.Name != null)
            {
                node.Attributes["JT_PROP_NAME"] = String.Format("{0}.{1};0;0:", node.Name, node.children.Count > 0 ? "asm" : "part");
            }

            if (node.GeometricSets.Length > 0)
            {
                node.Attributes["JT_LLPROP_SHAPEIMPL"] = node.GeometricSets;
            }

            var attributesCount = node.Attributes.Count;

            var keys = new List<int>(attributesCount);
            var values = new List<int>(attributesCount);

            foreach (var attribute in node.Attributes)
            {
                var key = attribute.Key;

                if (node.Name == null && key == "JT_PROP_NAME") continue;

                var value = attribute.Value;
                var valueTypeName = value.GetType().Name;

                if (valueTypeName != "String" && valueTypeName != "Int32" && valueTypeName != "Single" && valueTypeName != "DateTime" && valueTypeName != "GeometricSet[]")
                {
                    throw new Exception(String.Format("Only String, Int32, Single, DateTime and GeometricSet[] value types are allowed. Current value is {0}.", valueTypeName));
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

                var valueAsString = valueTypeName == "GeometricSet[]" ? ((GeometricSet[])value)[0].ToString() : value.ToString();
                var valueLookupKey = String.Format("{0}-{1}", valueTypeName, valueAsString);

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
                    }
                }

                values.Add(valueId);
            }

            propertyTableContents.Add(nodeElementId, new NodePropertyTable(keys, values));
        }
    }
}