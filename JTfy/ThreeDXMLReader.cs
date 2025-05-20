using System.Drawing;
using System.IO.Compression;
using System.Web;
using System.Xml;

namespace JTfy
{
    public static class ThreeDXMLReader
    {
        public static JTNode Read(string path, out int nodeCount, Action<float>? onProgress = null)
        {
            JTNode rootNode;

            using (var archive = ZipFile.OpenRead(path))
            {
                var archiveEntriesCount = archive.Entries.Count;

                ZipArchiveEntry? threeDXMLEntry = null;
                var partEntries = new Dictionary<string, ZipArchiveEntry>(archiveEntriesCount);

                for (int i = 0; i < archiveEntriesCount; ++i)
                {
                    var entry = archive.Entries[i];
                    var entryName = entry.FullName;
                    var entryNameExt = Path.GetExtension(entryName).ToLower();

                    if (entryName.Equals("manifest.xml", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.Load(entry.Open());

                        var rootElementText = xmlDoc.SelectSingleNode("//*[local-name()='Root']/text()");

                        if (rootElementText != null) threeDXMLEntry = archive.GetEntry(rootElementText.Value ?? "");
                    }

                    else if (entryNameExt == ".xml" || entryNameExt == ".3drep") partEntries[entryName] = entry;
                }

                if (threeDXMLEntry == null) throw new Exception(String.Format("{0} does not contain PRODUCT.3dxml file.", path));

                rootNode = BuildStructure(threeDXMLEntry.Open(), partEntries, out nodeCount, onProgress);
            }

            return rootNode;
        }

        private static JTNode BuildStructure(Stream threeDXMLFileStream, Dictionary<string, ZipArchiveEntry> partEntries, out int nodeCount, Action<float>? onProgress = null)
        {
            var threeDXMLDocument = new XmlDocument();
            threeDXMLDocument.Load(threeDXMLFileStream);

            var xmlNamespaceManager = new XmlNamespaceManager(threeDXMLDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ns", "http://www.3ds.com/xsd/3DXML");

            var referenceElements = threeDXMLDocument.SelectNodes("//*[local-name()='Reference3D' or local-name()='ReferenceRep']", xmlNamespaceManager);

            var referenceElementCount = referenceElements?.Count ?? 0;

            var instanceElements = threeDXMLDocument.SelectNodes("//*[local-name()='Instance3D' or local-name()='InstanceRep']", xmlNamespaceManager);

            var instanceElementsCount = instanceElements?.Count ?? 0;

            var totalCount = referenceElementCount + instanceElementsCount;
            var progressCounter = 0;

            var nodes = new Dictionary<string, JTNode>(referenceElementCount);

            for (int i = 0; i < referenceElementCount; ++i)
            {
                onProgress?.Invoke(++progressCounter / (float)totalCount);

                var reference3DElement = referenceElements?[i];

                if (reference3DElement == null) continue;

                var idAttributeValue = reference3DElement.Attributes?["id"]?.Value;

                if (idAttributeValue == null) continue;

                nodes[idAttributeValue] = XMLElement2Node(reference3DElement, partEntries);
            }

            for (int i = 0, c = instanceElementsCount; i < c; ++i)
            {
                onProgress?.Invoke(++progressCounter / (float)totalCount);

                var instanceElement = instanceElements?[i];

                if (instanceElement == null) continue;

                var childNodes = instanceElement.ChildNodes;

                string? aggregatedBy = null;
                string? instanceOf = null;
                string? relativeMatrix = null;

                for (int childNodeIndex = 0, childNodeCount = childNodes.Count; childNodeIndex < childNodeCount; ++childNodeIndex)
                {
                    var childNode = childNodes[childNodeIndex];

                    switch (childNode?.Name)
                    {
                        case "IsAggregatedBy": aggregatedBy = childNode.InnerText; break;
                        case "IsInstanceOf": instanceOf = childNode.InnerText; break;
                        case "RelativeMatrix": relativeMatrix = childNode.InnerText; break;
                    }
                }

                if (aggregatedBy == null || instanceOf == null || !nodes.ContainsKey(aggregatedBy) || !nodes.ContainsKey(instanceOf)) continue;

                float[]? transformationMatrix = null;

                if (relativeMatrix != null && relativeMatrix != "1 0 0 0 1 0 0 0 1 0 0 0")
                {
                    transformationMatrix = ConstUtils.IndentityMatrix;

                    var relativeMatrixValues = relativeMatrix.Trim().Split([' ']);

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
                    //Number = tempNode.Number,
                    TransformationMatrix = transformationMatrix
                };

                foreach (var attribute in tempNode.Attributes)
                {
                    nodeInstance.Attributes[$"I: {attribute.Key}"] = attribute.Value;
                }

                nodes[aggregatedBy].Children.Add(nodeInstance);
            }

            var productStructureElement = threeDXMLDocument.SelectSingleNode("//ns:ProductStructure", xmlNamespaceManager);
            var rootId = productStructureElement?.Attributes?["root"]?.Value ?? "";

            nodeCount = nodes.Count;

            return nodes[rootId];
        }

        private static JTNode XMLElement2Node(XmlNode referenceElement, Dictionary<string, ZipArchiveEntry> partEntries)
        {
            var referenceElementAttributes = referenceElement.Attributes;

            var node = new JTNode()
            {
                ID = int.Parse(referenceElementAttributes?["id"]?.Value ?? ""),
                Number = referenceElementAttributes?["name"]?.Value,
                Attributes = ExtractAttributes(referenceElement.ChildNodes)
            };

            if (node.Attributes.TryGetValue("V_Name", out var value))
                node.Name = value.ToString();

            if (referenceElement.Name == "ReferenceRep")
            {
                var partFileName = referenceElement?.Attributes?["associatedFile"]?.Value.Replace("urn:3DXML:", "") ?? "";

                if (partEntries.TryGetValue(partFileName, out var partEntry))
                {
                    using var partFileStream = partEntry.Open();

                    node.GeometricSets = GetGeometricSets(partFileStream);
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

            catch (Exception) { return []; }

            var xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ns", "http://www.3ds.com/xsd/3DXML");

            var geometricSets = new List<GeometricSet>();

            var repElements = xmlDocument.SelectNodes("//ns:Rep[ns:Faces/ns:Face[@fans or @strips or @triangles]][ns:VertexBuffer/ns:Positions]", xmlNamespaceManager);

            for (int repElementIndex = 0, repElementCount = repElements?.Count ?? 0; repElementIndex < repElementCount; ++repElementIndex)
            {
                var repElement = repElements?[repElementIndex];

                var positionStrings = repElement?.SelectSingleNode("./ns:VertexBuffer/ns:Positions/text()", xmlNamespaceManager)?.Value?.Trim().Split([',']);

                var normalsElement = repElement?.SelectSingleNode("./ns:VertexBuffer/ns:Normals/text()", xmlNamespaceManager);
                var normalStrings = normalsElement?.Value?.Trim().Split([',']);

                var positionsCount = positionStrings?.Length ?? 0;

                var positions = new float[positionsCount][];
                var normals = normalStrings == null ? null : new float[positionsCount][];

                for (int positionIndex = 0; positionIndex < positionsCount; ++positionIndex)
                {
                    var positionComponents = positionStrings?[positionIndex].Trim().Split([' ']);

                    if (positionComponents != null)
                    {
                        positions[positionIndex] =
                        [
                            float.Parse(positionComponents[0]),
                            float.Parse(positionComponents[1]),
                            float.Parse(positionComponents[2])
                        ];
                    }

                    if (normals != null)
                    {
                        var normalComponents = normalStrings?[positionIndex].Trim().Split([' ']);

                        if (normalComponents != null)
                        {
                            normals[positionIndex] =
                            [
                                float.Parse(normalComponents[0]),
                                float.Parse(normalComponents[1]),
                                float.Parse(normalComponents[2])
                            ];
                        }
                    }
                }

                var faceElements = repElement?.SelectNodes("./ns:Faces/ns:Face", xmlNamespaceManager);

                for (int faceIndex = 0, faceCount = faceElements?.Count ?? 0; faceIndex < faceCount; ++faceIndex)
                {
                    var faceElement = faceElements?[faceIndex];
                    var faceElementAttributes = faceElement?.Attributes;

                    var triStrips = new List<int[]>();

                    for (int attributeIndex = 0, attributeCount = faceElementAttributes?.Count ?? 0; attributeIndex < attributeCount; ++attributeIndex)
                    {
                        var attribute = faceElementAttributes?[attributeIndex];

                        switch (attribute?.Name)
                        {
                            case "strips":
                                {

                                    var stripsAttribute = attribute;
                                    var stripStrings = stripsAttribute != null ? stripsAttribute.Value.Trim().Split([',']) : [];

                                    for (int stripIndex = 0, stripCount = stripStrings.Length; stripIndex < stripCount; ++stripIndex)
                                    {
                                        var stripIndexStrings = stripStrings[stripIndex].Trim().Split([' ']);

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
                                    var fanStrings = fansAttribute != null ? fansAttribute.Value.Trim().Split([',']) : [];

                                    for (int fanIndex = 0, fanCount = fanStrings.Length; fanIndex < fanCount; ++fanIndex)
                                    {
                                        var fanIndexStrings = new List<string>(fanStrings[fanIndex].Trim().Split([' ']));

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
                                                    triStrips.Add([.. triStripIndices]);
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

                                        if (triStripIndices.Count > 0) triStrips.Add([.. triStripIndices]);
                                    }

                                    break;
                                }


                            case "triangles":
                                {

                                    var trianglesAttribute = attribute;
                                    var trianglesIndexStrings = trianglesAttribute != null ? trianglesAttribute.Value.Trim().Split([' ']) : [];

                                    for (int triangleIndexIndex = 2, triangleIndexCount = trianglesIndexStrings.Length; triangleIndexIndex < triangleIndexCount; triangleIndexIndex += 3)
                                    {
                                        triStrips.Add(
                                        [
                                            int.Parse(trianglesIndexStrings[triangleIndexIndex - 2]),
                                            int.Parse(trianglesIndexStrings[triangleIndexIndex - 1]),
                                            int.Parse(trianglesIndexStrings[triangleIndexIndex])
                                        ]);
                                    }

                                    break;
                                }
                        }
                    }

                    if (triStrips.Count == 0 || positions.Length == 0) continue;

                    var geometricSet = new GeometricSet([.. triStrips], positions)
                    {
                        Normals = normals
                    };

                    var colorElement = faceElement?.SelectSingleNode("./ns:SurfaceAttributes/ns:Color[@alpha and @red and @green and @blue]", xmlNamespaceManager);
                    if (colorElement != null)
                    {
                        var attributes = colorElement.Attributes;
                        var a = (int)(float.Parse(attributes?["alpha"]?.Value ?? "1") * 255);
                        var r = (int)(float.Parse(attributes?["red"]?.Value ?? "0.5") * 255);
                        var g = (int)(float.Parse(attributes?["green"]?.Value ?? "0.5") * 255);
                        var b = (int)(float.Parse(attributes?["blue"]?.Value ?? "0.5") * 255);

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

            return geometricSets.Count == 0 ? [] : [.. geometricSets];
        }

        private static Dictionary<string, object> ExtractAttributes(XmlNodeList attributeNodes)
        {
            var attributeCount = attributeNodes.Count;
            var attributes = new Dictionary<string, object>(attributeCount);

            for (int i = 0; i < attributeCount; ++i)
            {
                var attributeNode = attributeNodes[i];

                if (attributeNode == null) continue;

                var attributeNodeName = attributeNode.Name;

                if (attributeNodeName == "IsAggregatedBy" || attributeNodeName == "IsInstanceOf") continue;

                var childNodes = attributeNode.ChildNodes;

                if (childNodes == null) continue;

                if (childNodes.Count == 0) continue;

                if (childNodes[0]?.NodeType == XmlNodeType.Text)
                {
                    var value = HttpUtility.HtmlDecode(attributeNode.InnerText);

                    // possibly perform value conversion from string to int, float, date?

                    attributes[attributeNode.Name] = value;
                }

                else
                {
                    var childAttributes = ExtractAttributes(childNodes);

                    foreach (var childAttribute in childAttributes)
                    {
                        attributes[childAttribute.Key] = childAttribute.Value;
                    }
                }
            }

            return attributes;
        }
    }
}
