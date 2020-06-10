using System.IO;

namespace JTfy
{
    class Program
    {
        static void Main(string[] args)
        {
            //Save();
            //Load();
            //Read3DXML();

            var sourcePath = args[0];
            var destinationPath = Path.Combine(Path.GetDirectoryName(sourcePath), Path.GetFileNameWithoutExtension(sourcePath) + ".jt");

            ThreeDXMLReader.Read(sourcePath).Save(destinationPath, false);
        }

        //static void Read3DXML()
        //{
        //ThreeDXMLReader.Read(@"C:\Users\mtadara1\Desktop\Assy Door Carrier Nov 2018.3dxml").Save(@"C:\Users\mtadara1\Desktop\Assy Door Carrier Nov 2018.jt", false);
        //ThreeDXMLReader.Read(@"C:\Users\mtadara1\Desktop\3DXML_Files\fender2\L8B2-16E004-A-DMU-01.3DXML").Save(@"C:\Users\mtadara1\Desktop\fender_from_3dxml.jt");
        //ThreeDXMLReader.Read(@"\\gal71836.fs3.util.jlrint.com\hq\Manufacturing\AME\VME\sys_root\VIRTUAL_SIMULATIONS\PROJECTS\L66321MY\UNV1\L66321MYUNV1TF0001\Product\NEW_STEERING COLUMN FOR L66321MYUNV1TF0001.3dxml").Save(@"C:\Users\mtadara1\Desktop\NEW_STEERING COLUMN FOR L66321MYUNV1TF0001.jt", false);
        //ThreeDXMLReader.Read(@"C:\Users\mtadara1\Desktop\3DXML_Files\body3\F-L8B2-00000-DA-UEB2-013501-000.3DXML").Save(@"C:\Users\mtadara1\Desktop\F-L8B2-00000-DA-UEB2-013501-000.jt");
        //ThreeDXMLReader.Read(@"C:\Users\mtadara1\Desktop\3DXML_Files\body3\F-L8B2-00000-DA-UEB2-013501-000.3DXML").Save(@"C:\Users\mtadara1\Desktop\body_from_3dxml.jt", false);

        //var threeDRepContent = new StreamReader(@"C:\Users\mtadara1\Desktop\3DXML_Files\Pedal\F-HPLA-2D094-BA-UEC1-060602-000\CPLA-2D094-A-INS-04_2_b83470b6_274_5a853c46_15e3a.xml").ReadToEnd();
        //var threeDRepContent = new StreamReader(@"C:\Users\mtadara1\Desktop\3DXML_Files\Pedal\F-HPLA-2D094-BA-UEC1-060602-000\Representation1408425_2_b83470b6_274_5a853c46_15e63.xml").ReadToEnd();
        //var threeDRepContent = new StreamReader(@"C:\Users\mtadara1\Desktop\3DXML_Files\3dxml_fender\ExportFile\Representation123_2_b83470b6_2060_5a784c11_34db8.xml").ReadToEnd();

        /*new JTNode()
        {
            Name = "Test",
            GeometricSets = ThreeDXMLReader.GetGeometricSets(threeDRepContent)
        }.Save(@"C:\Users\mtadara1\Desktop\out_SAVE_from_3dxml_part_file.jt");*/
        //}

        /*static void Save()
        {
            var root = new JTNode()
            {
                Name = "Richard",
                Attributes = new Dictionary<string, object>()
                {
                    {"prop11", DateTime.Now},
                    {"prop22", 22},
                    {"prop33", 22.22f},
                    {"prop44", "asdASD"}
                }
            };

            var childNode1 = new JTNode()
            {
                Name = "Mab",
                Attributes = new Dictionary<string, object>()
                {
                    {"some  date", DateTime.Now}
                }
            };

            var childNode2 = new JTNode()
            {
                Name = "Asd",
                Attributes = new Dictionary<string, object>()
                {
                    {"prop1", DateTime.Now},
                    {"prop2", 2},
                    {"prop3", 2.2f},
                    {"prop4", "asd"}
                }
            };

            var instancedNode = new JTNode()
            {
                Name = "Hoar",

                Attributes = new Dictionary<string, object>()
                {
                    {"prop88", DateTime.Now},
                    {"prop99", 201},
                    {"prop1111", 42.2f},
                    {"prop2222", "krm"}
                },

                GeometricSets = new GeometricSet[]
                {
                    new GeometricSet
                    (
                        new int[][]
                        {
                            //new int[] { 0, 1, 2, 3 }
                            new int[] { 0, 1, 2 }
                        },

                        new float[][]
                        {
                            new float[] { 0, 0, 0 },
                            new float[] { 100, 0, 0 },
                            new float[] { 0, 0, 100 }//,
                            //new float[] { 0, 100, 0 }
                        }
                    )
                    {
                        Colour = Color.FromArgb(128, Color.CornflowerBlue),
                        Normals = new float[][]
                        {
                            new float[] { 0, -1, 0 },
                            new float[] { 0, -1, 0 },
                            new float[] { 0, -1, 0 }//,
                            //new float[] { 0, -1, 0 }
                        }
                    },

                    new GeometricSet
                    (
                        new int[][]
                        {
                            new int[] { 0, 1, 2 }
                        },

                        new float[][]
                        {
                            new float[] { 100, 100, 100 },
                            new float[] { 200, 100, 100 },
                            new float[] { 100, 100, 200 }
                        }
                    )
                    {
                        Colour = Color.ForestGreen,
                        Normals = new float[][]
                        {
                            new float[] { 0, -1, 0 },
                            new float[] { 0, -1, 0 },
                            new float[] { 0, -1, 0 }
                        }
                    },
                }
            };


            childNode1.Children = new List<JTNode> { instancedNode, instancedNode };
            childNode1.TransformationMatrix = new float[]
            {
                1,0,0,0,
                0,1,0,0,
                0,0,1,0,
                10,10,10,1
            };

            root.Children = new List<JTNode>
            {
                //childNode2,
                //childNode1,
                instancedNode
            };

            root.Save(@"C:\Users\mtadara1\Desktop\out_SAVE_VIA_SYSTEM.jt");
        }*/

        /*static void Load()
        {
            // 8.1
            Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\JTTest\81\DS_L405_040102_C01_18_FR_SUSP_LINKS___ARMS_20.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\JTTest\81\DS_L405_040102_C01_18_FR_SUSP_LINKS___ARMS\W780052_S_INS_01_NUT_WSHR_M10_HC_PTP_FL10_2.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\L8B2-16015-A-INS-01_L663_FENDER_OUTER_RH.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\test.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\test81.jt");

            // 9.5
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\JTTest\DS_L405_040102_C01_18_FR_SUSP_LINKS___ARMS\W780052_S_INS_01_NUT_WSHR_M10_HC_PTP_FL10_2.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\JTTest\DS_L405_040102_C01_18_FR_SUSP_LINKS___ARMS_20.jt");

            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\JTTest\2 Fixings structure\DS_PLA_060301_C02_13_L405_FRT_ROTOR___SHIELD_20__46.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\L405_Digital_Buck.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\JTTest\test.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\JTTest\Radial_Engine_Pip\Spark_Plug.jt");

            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\out.jt");


            var jtFileArray = new Byte[jtFileStream.Length];
            jtFileStream.Read(jtFileArray, 0, jtFileArray.Length);

            jtFileStream = new MemoryStream(jtFileArray);


            // File Header

            var fileHeader = new FileHeader(jtFileStream);

            // END File Header

            // TOC Segment

            jtFileStream.Seek(fileHeader.TOCOffset, SeekOrigin.Begin);

            var tocSegment = new TOCSegment(jtFileStream);

            // END TOC Segment

            // Data Segments

            var tocEntriesCount = tocSegment.TOCEntries.Length;

            var dataSegmentHeaders = new SegmentHeader[tocEntriesCount];
            var logicElementHeadersZLIB = new LogicElementHeaderZLIB[tocEntriesCount];
            var dataSegments = new BaseDataStructure[tocEntriesCount];

            for (int i = 0; i < tocEntriesCount; ++i)
            {
                var tocEntry = tocSegment.TOCEntries[i];

                jtFileStream.Position = tocEntry.SegmentOffset;

                var segmentData = StreamUtils.ReadBytes(jtFileStream, tocEntry.SegmentLength, false);

                using (var dataSegmentStream = new MemoryStream(segmentData))
                {
                    var stream = dataSegmentStream;

                    var segmentHeader = new SegmentHeader(dataSegmentStream);

                    dataSegmentHeaders[i] = segmentHeader;

                    LogicElementHeaderZLIB logicElementHeaderZLIB = null;

                    var segmentType = segmentHeader.SegmentType;

                    if ((segmentType > 0 && segmentType < 5) || segmentType == 17 || segmentType == 18 || segmentType == 20 || segmentType == 24)
                    {
                        logicElementHeaderZLIB = new LogicElementHeaderZLIB(dataSegmentStream);

                        if (logicElementHeaderZLIB.CompressionFlag == 2 && logicElementHeaderZLIB.CompressionAlgorithm == 2)
                        {

                            var compressedData = StreamUtils.ReadBytes(dataSegmentStream, (int)(dataSegmentStream.Length - dataSegmentStream.Position), false);

                            var decompressedData = CompressionUtils.Decompress(compressedData);

                            stream = new MemoryStream(decompressedData);
                        }
                    }

                    logicElementHeadersZLIB[i] = logicElementHeaderZLIB;

                    switch (segmentType)
                    {
                        case 1: // LSG Segment type
                            {
                                dataSegments[i] = new LSGSegment(stream);

                                break;
                            }

                        case 4:
                            {
                                dataSegments[i] = new MetaDataSegment(stream);

                                break;
                            }

                        case 6:
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 16:
                            {
                                //break;
                                dataSegments[i] = new ShapeLODSegment(stream);

                                break;
                            }

                        default:
                            {
                                //break;
                                throw new NotImplementedException(String.Format("Case not defined for Segment Type {0}", segmentType));
                            }
                    }
                }
            }

            // END Data Segments

#if DEBUG*/
        /*foreach (var dataSegment in dataSegments)
        {
            if (dataSegment.GetType() != typeof(LSGSegment)) continue;

            Debug.WriteLine("");

            foreach (var graphElement in ((LSGSegment)dataSegment).GraphElements)
            {
                if (!graphElement.GetType().IsSubclassOf(typeof(BaseNodeElement))) continue;

                Debug.WriteLine("");
                Debug.WriteLine(graphElement.GetType().Name);
                Debug.WriteLine("");

                if(true)
                {
                    foreach (var attributeId in ((BaseNodeElement)graphElement).AttributeObjectIds)
                    {
                        Debug.WriteLine("\t" + ((LSGSegment)dataSegment).GraphElements.Where(el => (el.GetType().IsSubclassOf(typeof(BaseAttributeElement)) ? ((BaseAttributeElement)el).ObjectId : ((BaseNodeElement)el).ObjectId) == attributeId).First().GetType().Name);
                    }
                }

                if (false)
                {
                    var propertyTable = ((LSGSegment)dataSegment).PropertyTable;

                    var propertiesTableIndex = Array.IndexOf(propertyTable.NodeObjectIDs, graphElement.GetType().IsSubclassOf(typeof(BaseAttributeElement)) ? ((BaseAttributeElement)graphElement).ObjectId : ((BaseNodeElement)graphElement).ObjectId);

                    if (propertiesTableIndex == -1) continue;

                    var propertiesTable = propertyTable.NodePropertyTables[propertiesTableIndex];

                    for (int i = 0, c = propertiesTable.KeyPropertyAtomObjectIDs.Count; i < c; ++i)
                    {
                        Debug.WriteLine(String.Format("\t{0}\t{1}",
                            ((LSGSegment)dataSegment).PropertyAtomElements.Where(propEl => propEl.ObjectID == propertiesTable.KeyPropertyAtomObjectIDs[i]).First(),
                            ((LSGSegment)dataSegment).PropertyAtomElements.Where(propEl => propEl.ObjectID == propertiesTable.ValuePropertyAtomObjectIDs[i]).First()
                        ));
                    }
                }
            }

            Debug.WriteLine("");
        }*/

        /*var geometricSets = new List<GeometricSet>();

        foreach (var dataSegment in dataSegments)
        {
            if (dataSegment.GetType() != typeof(ShapeLODSegment)) continue;

            var vertexBasedShapeCompressedRepData = ((TriStripSetShapeLODElement)((ShapeLODSegment)dataSegment).ShapeLODElement).VertexBasedShapeCompressedRepData;

            Debug.WriteLine("");
            Debug.WriteLine("Read");
            Debug.WriteLine(String.Join(",", vertexBasedShapeCompressedRepData.LosslessCompressedRawVertexData.VertexData));
            Debug.WriteLine(String.Join(",", vertexBasedShapeCompressedRepData.PrimitiveListIndices));
            Debug.WriteLine(String.Join(",", vertexBasedShapeCompressedRepData.LosslessCompressedRawVertexData.CompressedVertexData));
            Debug.WriteLine("");

            geometricSets.Add(new GeometricSet(vertexBasedShapeCompressedRepData.TriStrips, vertexBasedShapeCompressedRepData.Positions) { Normals = vertexBasedShapeCompressedRepData.Normals });
        }

        new JTNode()
        {
            Name = "Test",
            GeometricSets = geometricSets.ToArray()
        }.Save(@"C:\Users\mtadara1\Desktop\test.jt");*/
        /*#endif
                }*/
    }
}