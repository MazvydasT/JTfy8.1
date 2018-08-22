using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace JTfy
{
    public class GeometricSet
    {
        private float[][] positions = new float[0][];
        public float[][] Positions { get { return positions; } private set { positions = value == null ? new float[0][] : value; } }
        
        public float[][] Normals { get; set; }

        private int[][] triStrips = new int[0][];
        public int[][] TriStrips { get { return triStrips; } private set { triStrips = value == null ? new int[0][] : value; } }

        private Color colour = RandomGenUtils.NextColour();
        public Color Colour { get { return colour; } set { colour = value; } }

        private int id = IdGenUtils.NextId;
        public int ID { get { return id; } set { id = value; } }

        public int TriangleCount
        {
            get
            {
                var vertexCount = 0;

                for (int i = 0, c = TriStrips.Length; i < c; ++i)
                {
                    vertexCount += TriStrips[i].Length - 2;
                }

                return vertexCount;
            }
        }

        public float Area
        {
            get
            {
                double area = 0;

                for (int triStripIndex = 0, triStripCount = TriStrips.Length; triStripIndex < triStripCount; ++triStripIndex)
                {
                    var triStrip = TriStrips[triStripIndex];

                    for (int i = 0, c = triStrip.Length - 2; i < c; ++i)
                    {
                        area += CalcUtils.GetTriangleArea(
                            Positions[triStrip[i]],
                            Positions[triStrip[i + 1]],
                            Positions[triStrip[i + 2]]
                        );
                    }
                }

                return (float)area;
            }
        }

        public int Size
        {
            get
            {
                int size = Positions.Length * 4 * (Normals == null ? 1 : 2);

                for (int i = 0, c = TriStrips.Length; i < c; ++i)
                {
                    size += TriStrips[i].Length * 4;
                }

                return size;
            }
        }

        public CoordF32 Center
        {
            get
            {
                var boundingBox = UntransformedBoundingBox;
                var maxCorner = boundingBox.MaxCorner;
                var minCorner = boundingBox.MinCorner;

                return new CoordF32(
                    maxCorner.X - minCorner.X,
                    maxCorner.Y - minCorner.Y,
                    maxCorner.Z - minCorner.Z
                );
            }
        }

        public BBoxF32 UntransformedBoundingBox
        {
            get
            {
                float minX = 0, minY = 0, minZ = 0, maxX = 0, maxY = 0, maxZ = 0;

                for (int i = 0, c = Positions.Length; i < c; ++i)
                {
                    var position = Positions[i];
                    var x = position[0];
                    var y = position[1];
                    var z = position[2];

                    if (i == 0)
                    {
                        minX = maxX = x;
                        minY = maxY = y;
                        minZ = maxZ = z;
                    }

                    else
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (z < minZ) minZ = z;

                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                        if (z > maxZ) maxZ = z;
                    }
                }

                return new BBoxF32(minX, minY, minZ, maxX, maxY, maxZ);
            }
        }

        public GeometricSet(int[][] triStrips, float[][] positions)
        {
            TriStrips = triStrips;
            Positions = positions;
        }

        public override string ToString()
        {
            /*var stringList = new List<string>(Positions.Length * (Normals == null ? 1 : 2) + TriStrips.Length);

            for (int i = 0, c = Positions.Length; i < c; ++i)
            {
                stringList.Add(String.Join(",", Positions[i]));

                if (Normals != null)
                {
                    stringList.Add(String.Join(",", Normals[i]));
                }
            }

            for (int i = 0, c = TriStrips.Length; i < c; ++i)
            {
                stringList.Add(String.Join(",", TriStrips[i]));
            }

            return String.Join("|", stringList) + Colour.ToString();*/

            return ID.ToString();
        }
    }
}