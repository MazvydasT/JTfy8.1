using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace JTfy
{
    public static class ThreeDXMLReader
    {
        public static JTNode Read(string path)
        {
            JTNode rootNode = null;

            using (var archive = ZipFile.OpenRead(path))
            {
                var archiveEntriesCount = archive.Entries.Count;

                ZipArchiveEntry threeDXMLEntry = null;
                var partEntries = new Dictionary<string, ZipArchiveEntry>(archiveEntriesCount);

                for (int i = 0; i < archiveEntriesCount; ++i)
                {
                    var entry = archive.Entries[i];
                    var entryName = entry.FullName;
                    var entryNameExt = Path.GetExtension(entryName).ToLower();

                    if (entryName.ToLower() == "manifest.xml")
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.Load(entry.Open());

                        var rootElementText = xmlDoc.SelectSingleNode("//*[local-name()='Root']/text()");

                        if (rootElementText != null) threeDXMLEntry = archive.GetEntry(rootElementText.Value);
                    }

                    else if (entryNameExt == ".xml" || entryNameExt == ".3drep") partEntries[entryName] = entry;
                }

                if (threeDXMLEntry == null) throw new Exception(String.Format("{0} does not contain PRODUCT.3dxml file.", path));

                rootNode = BuildStructure(threeDXMLEntry.Open(), partEntries);
            }

            return rootNode;
        }

        private static JTNode BuildStructure(Stream threeDXMLFileStream, Dictionary<string, ZipArchiveEntry> partEntries)
        {
            var threeDXMLDocument = new XmlDocument();
            threeDXMLDocument.Load(threeDXMLFileStream);

            var xmlNamespaceManager = new XmlNamespaceManager(threeDXMLDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ns", "http://www.3ds.com/xsd/3DXML");

            var referenceElements = threeDXMLDocument.SelectNodes("//*[local-name()='Reference3D' or local-name()='ReferenceRep']", xmlNamespaceManager);

            var referenceElementCount = referenceElements.Count;

            var nodes = new Dictionary<string, JTNode>(referenceElementCount);

            for (int i = 0; i < referenceElementCount; ++i)
            {
                var reference3DElement = referenceElements[i];

                nodes[reference3DElement.Attributes["id"].Value] = XMLElement2Node(reference3DElement, partEntries);
            }

            var instanceElements = threeDXMLDocument.SelectNodes("//*[local-name()='Instance3D' or local-name()='InstanceRep']", xmlNamespaceManager);

            for (int i = 0, c = instanceElements.Count; i < c; ++i)
            {
                var instanceElement = instanceElements[i];
                
                var childNodes = instanceElement.ChildNodes;
                
                string aggregatedBy = null;
                string instanceOf = null;
                string relativeMatrix = null;

                for (int childNodeIndex = 0, childNodeCount = childNodes.Count; childNodeIndex < childNodeCount; ++childNodeIndex)
                {
                    var childNode = childNodes[childNodeIndex];

                    switch (childNode.Name)
                    {
                        case "IsAggregatedBy": aggregatedBy = childNode.InnerText; break;
                        case "IsInstanceOf": instanceOf = childNode.InnerText; break;
                        case "RelativeMatrix": relativeMatrix = childNode.InnerText; break;
                    }
                }

                if (aggregatedBy == null || instanceOf == null || !nodes.ContainsKey(aggregatedBy) || !nodes.ContainsKey(instanceOf)) continue;
                
                float[] transformationMatrix = null;

                if (relativeMatrix != null && relativeMatrix != "1 0 0 0 1 0 0 0 1 0 0 0")
                {
                    transformationMatrix = ConstUtils.IndentityMatrix;

                    var relativeMatrixValues = relativeMatrix.Trim().Split(new char[]{' '});

                    var offset = 0;

                    for (int matrixValueIndex = 0, matrixValueCount = relativeMatrixValues.Length; matrixValueIndex < matrixValueCount; matrixValueIndex += 3)
                    {
                        transformationMatrix[matrixValueIndex + offset] = float.Parse(relativeMatrixValues[matrixValueIndex]);
                        transformationMatrix[matrixValueIndex + 1 + offset] = float.Parse(relativeMatrixValues[matrixValueIndex + 1]);
                        transformationMatrix[matrixValueIndex + 2 + offset] = float.Parse(relativeMatrixValues[matrixValueIndex + 2]);

                        ++offset;
                    }
                }

                var tempNode = XMLElement2Node(instanceElement, partEntries);
                
                var nodeInstance = new JTNode(nodes[instanceOf])
                {
                    Name = tempNode.Name,
                    TransformationMatrix = transformationMatrix
                };

                foreach (var attribute in tempNode.Attributes)
                {
                    nodeInstance.Attributes[attribute.Key] = attribute.Value;
                }

                nodes[aggregatedBy].Children.Add(nodeInstance);
            }

            var productStructureElement = threeDXMLDocument.SelectSingleNode("//ns:ProductStructure", xmlNamespaceManager);
            var rootId = productStructureElement.Attributes["root"].Value;

            return nodes[rootId];
        }

        private static JTNode XMLElement2Node(XmlNode referenceElement, Dictionary<string, ZipArchiveEntry> partEntries)
        {
            var referenceElementAttributes = referenceElement.Attributes;

            var node = new JTNode()
            {
                ID = int.Parse(referenceElementAttributes["id"].Value),
                Name = referenceElementAttributes["name"].Value,
                Attributes = ExtractAttributes(referenceElement.ChildNodes)
            };

            if (referenceElement.Name == "ReferenceRep")
            {
                var partFileName = referenceElement.Attributes["associatedFile"].Value.Replace("urn:3DXML:", "");

                if (partEntries.ContainsKey(partFileName))
                {
                    using (var partFileStream = partEntries[partFileName].Open())
                    {
                        node.GeometricSets = GetGeometricSets(partFileStream);
                    }
                }
            }

            return node;
        }
        
        public static GeometricSet[] GetGeometricSets(Stream stream)
        {
            var xmlDocument = new XmlDocument();

            try
            {
                xmlDocument.Load(stream);
            }

            catch (Exception) { return null; }

            var xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ns", "http://www.3ds.com/xsd/3DXML");

            var geometricSets = new List<GeometricSet>();

            var repElements = xmlDocument.SelectNodes("//ns:Rep[ns:Faces/ns:Face[@fans or @strips or @triangles]][ns:VertexBuffer/ns:Positions]", xmlNamespaceManager);

            for (int repElementIndex = 0, repElementCount = repElements.Count; repElementIndex < repElementCount; ++repElementIndex)
            {
                var repElement = repElements[repElementIndex];

                var positionStrings = repElement.SelectSingleNode("./ns:VertexBuffer/ns:Positions/text()", xmlNamespaceManager).Value.Trim().Split(new char[] { ',' });

                var normalsElement = repElement.SelectSingleNode("./ns:VertexBuffer/ns:Normals/text()", xmlNamespaceManager);
                var normalStrings = normalsElement == null ? null : normalsElement.Value.Trim().Split(new char[] { ',' });
                
                var positionsCount = positionStrings.Length;

                var positions = new float[positionsCount][];
                var normals = normalStrings == null ? null : new float[positionsCount][];

                for (int positionIndex = 0; positionIndex < positionsCount; ++positionIndex)
                {
                    var positionComponents = positionStrings[positionIndex].Trim().Split(new char[] {' '});

                    positions[positionIndex] = new float[]
                    {
                        float.Parse(positionComponents[0]),
                        float.Parse(positionComponents[1]),
                        float.Parse(positionComponents[2])
                    };

                    if (normals != null)
                    {
                    	var normalComponents = normalStrings[positionIndex].Trim().Split(new char[]{' '});

                        normals[positionIndex] = new float[]
                        {
                            float.Parse(normalComponents[0]),
                            float.Parse(normalComponents[1]),
                            float.Parse(normalComponents[2])
                        };
                    }
                }

                var faceElements = repElement.SelectNodes("./ns:Faces/ns:Face", xmlNamespaceManager);

                for (int faceIndex = 0, faceCount = faceElements.Count; faceIndex < faceCount; ++faceIndex)
                {
                    var faceElement = faceElements[faceIndex];
                    var faceElementAttributes = faceElement.Attributes;

                    var triStrips = new List<int[]>();

                    for (int attributeIndex = 0, attributeCount = faceElementAttributes.Count; attributeIndex < attributeCount; ++attributeIndex)
                    {
                        var attribute = faceElementAttributes[attributeIndex];

                        switch (attribute.Name)
                        {
                            case "strips":
                                {

                                    var stripsAttribute = attribute;
                                    var stripStrings = stripsAttribute != null ? stripsAttribute.Value.Trim().Split(new char[] { ',' }) : new string[0];

                                    for (int stripIndex = 0, stripCount = stripStrings.Length; stripIndex < stripCount; ++stripIndex)
                                    {
                                        var stripIndexStrings = stripStrings[stripIndex].Trim().Split(new char[] { ' ' });

                                        var stripIndexCount = stripIndexStrings.Length;

                                        if (stripIndexCount < 3) continue;

                                        var stripIndices = new int[stripIndexCount];

                                        for (int stripIndexIndex = 0; stripIndexIndex < stripIndexCount; ++stripIndexIndex)
                                        {
                                            stripIndices[stripIndexIndex] = int.Parse(stripIndexStrings[stripIndexIndex]);
                                        }

                                        triStrips.Add(stripIndices);
                                    }

                                    break;
                                }

                            case "fans":
                                {

                                    var fansAttribute = attribute;
                                    var fanStrings = fansAttribute != null ? fansAttribute.Value.Trim().Split(new char[] { ',' }) : new string[0];

                                    for (int fanIndex = 0, fanCount = fanStrings.Length; fanIndex < fanCount; ++fanIndex)
                                    {
                                        var fanIndexStrings = new List<string>(fanStrings[fanIndex].Trim().Split(new char[] { ' ' }));

                                        var fanIndexCount = fanIndexStrings.Count;

                                        if (fanIndexCount < 3) continue;

                                        var firstFanIndex = int.Parse(fanIndexStrings[0]);
                                        fanIndexStrings.RemoveAt(0);
                                        --fanIndexCount;

                                        var triStripIndices = new List<int>(5);

                                        for (int fanIndexIndex = 0; fanIndexIndex < fanIndexCount; ++fanIndexIndex)
                                        {
                                            if (fanIndexIndex == 0 || fanIndexIndex % 3 == 0)
                                            {
                                                if (triStripIndices.Count > 0)
                                                {
                                                    triStripIndices.Add(int.Parse(fanIndexStrings[fanIndexIndex]));
                                                    triStrips.Add(triStripIndices.ToArray());
                                                }

                                                triStripIndices.Clear();

                                                if (fanIndexCount - fanIndexIndex < 2) break;

                                                triStripIndices.Add(int.Parse(fanIndexStrings[fanIndexIndex]));
                                                triStripIndices.Add(int.Parse(fanIndexStrings[fanIndexIndex + 1]));
                                                triStripIndices.Add(firstFanIndex);

                                                ++fanIndexIndex;
                                            }

                                            else triStripIndices.Add(int.Parse(fanIndexStrings[fanIndexIndex]));
                                        }

                                        if (triStripIndices.Count > 0) triStrips.Add(triStripIndices.ToArray());
                                    }

                                    break;
                                }


                            case "triangles":
                                {

                                    var trianglesAttribute = attribute;
                                    var trianglesIndexStrings = trianglesAttribute != null ? trianglesAttribute.Value.Trim().Split(new char[] { ' ' }) : new string[0];

                                    for (int triangleIndexIndex = 2, triangleIndexCount = trianglesIndexStrings.Length; triangleIndexIndex < triangleIndexCount; triangleIndexIndex += 3)
                                    {
                                        triStrips.Add(new int[]
                                        {
                                            int.Parse(trianglesIndexStrings[triangleIndexIndex - 2]),
                                            int.Parse(trianglesIndexStrings[triangleIndexIndex - 1]),
                                            int.Parse(trianglesIndexStrings[triangleIndexIndex])
                                        });
                                    }

                                    break;
                                }
                        }
                    }

                    if (triStrips.Count == 0 || positions.Length == 0) continue;
                    
                    var geometricSet = new GeometricSet(triStrips.ToArray(), positions)
                    {
                        Normals = normals
                    };

                    var colorElement = faceElement.SelectSingleNode("./ns:SurfaceAttributes/ns:Color[@alpha and @red and @green and @blue]", xmlNamespaceManager);
                    if (colorElement != null)
                    {
                        var attributes = colorElement.Attributes;
                        var a = (int)(float.Parse(attributes["alpha"].Value) * 255);
                        var r = (int)(float.Parse(attributes["red"].Value) * 255);
                        var g = (int)(float.Parse(attributes["green"].Value) * 255);
                        var b = (int)(float.Parse(attributes["blue"].Value) * 255);

                        geometricSet.Colour = Color.FromArgb(a, r, g, b);
                    }

                    geometricSets.Add(geometricSet);
                }
            }

            /*geometricSets.Sort((a, b) =>
            {
                var alphaA = a.Colour.A;
                var alphaB = b.Colour.A;

                return alphaA > alphaB ? -1 : (alphaA == alphaB ? 0 : 1);
            });*/
            
            return geometricSets.Count == 0 ? null : geometricSets.ToArray();
        }

        private static Dictionary<string, object> ExtractAttributes(XmlNodeList attributeNodes)
        {
            var attributeCount = attributeNodes.Count;
            var attributes = new Dictionary<string, object>(attributeCount);

            for (int i = 0; i < attributeCount; ++i)
            {
                var attributeNode = attributeNodes[i];
                var attributeNodeName = attributeNode.Name;

                if (attributeNodeName == "IsAggregatedBy" || attributeNodeName == "IsInstanceOf") continue;

                var childNodes = attributeNode.ChildNodes;

                if (childNodes.Count == 0) continue;

                if (childNodes[0].NodeType == XmlNodeType.Text)
                {
                    var value = HttpUtility.HtmlDecode(attributeNode.InnerText);
                    
                    // possibly perform value conversion from string to int, float, date?

                    attributes[attributeNode.Name] = value;
                }

                else
                {
                    var childAttributes = ExtractAttributes(childNodes);

                    foreach(var childAttribute in childAttributes)
                    {
                        attributes[childAttribute.Key] = childAttribute.Value;
                    }
                }
            }

            return attributes;
        }
    }
}
