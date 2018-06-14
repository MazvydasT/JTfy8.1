using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

#if DEBUG
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
#endif

namespace JTfy
{
    class Program
    {
#if DEBUG
        static Byte[] decompressedOriginalSegment;
#endif

        static void Main(string[] args)
        {
            Save();
            //Load();
        }

        static void Save()
        {
            var root = new JTNode()
            {
                Name = "Richard"
            };

            var childNode1 = new JTNode()
            {
                Name = "Mab"
            };

            var childNode2 = new JTNode()
            {
                Name = "Asd"
            };

            var instancedNode = new JTNode()
            {
                Name = "Hoar",

                GeometricSets = new GeometricSet[]
                {
                    new GeometricSet
                    (
                        new int[][]
                        {
                            new int[] { 0, 1, 2 }
                        },

                        new float[][]
                        {
                            new float[] { 0, 0, 0 },
                            new float[] { 100, 0, 0 },
                            new float[] { 0, 0, 100 }
                        }
                    )
                    {
                        //Colour = Color.CornflowerBlue,
                        Normals = new float[][]
                        {
                            new float[] { 0, -1, 0 },
                            new float[] { 0, -1, 0 },
                            new float[] { 0, -1, 0 }
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
                        //Colour = Color.ForestGreen,
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
                childNode2,
                childNode1,
                instancedNode
            };

            root.Save(@"C:\Users\mtadara1\Desktop\out_SAVE_VIA_SYSTEM.jt");
        }

        static void Load()
        {
            // 8.1
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\JTTest\81\DS_L405_040102_C01_18_FR_SUSP_LINKS___ARMS_20.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\JTTest\81\DS_L405_040102_C01_18_FR_SUSP_LINKS___ARMS\W780052_S_INS_01_NUT_WSHR_M10_HC_PTP_FL10_2.jt");
            Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\out_SAVE_VIA_SYSTEM.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\JTTest\81\plane.jt");
            //Stream jtFileStream = File.OpenRead(@"C:\Users\mtadara1\Desktop\transparent_nut.jt");

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

#if DEBUG
            var testVersion = Encoding.ASCII.GetString(fileHeader.Version);
#endif

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

#if DEBUG
                var segTypeFromSegAttributes = tocEntry.SegmentAttributes >> 24;
#endif

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

#if DEBUG
                            decompressedOriginalSegment = decompressedData;
#endif

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
                            {
                                dataSegments[i] = new ShapeLODSegment(stream);

                                break;
                            }

                        default:
                            {
                                throw new NotImplementedException(String.Format("Case not defined for Segment Type {0}", segmentType));
                            }
                    }
                }
            }

            // END Data Segments
        }
    }
}