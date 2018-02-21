using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;

namespace Henzai.Geometry
{
    public static class GeometryFactory
    {
        public static Mesh<VertexPositionTexture> generateTexturedCube()
        {
            VertexPositionTexture[] cubeVerticies =
                new VertexPositionTexture[]
                {
                    // Top
                    new VertexPositionTexture(new Vector3(-1.0f, +1.0f, -1.0f), new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3(+1.0f, +1.0f, -1.0f), new Vector2(1, 0)),
                    new VertexPositionTexture(new Vector3(+1.0f, +1.0f, +1.0f), new Vector2(1, 1)),
                    new VertexPositionTexture(new Vector3(-1.0f, +1.0f, +1.0f), new Vector2(0, 1)),
                    // Bottom                                                             
                    new VertexPositionTexture(new Vector3(-1.0f,-1.0f, +1.0f),  new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3(+1.0f,-1.0f, +1.0f),  new Vector2(1, 0)),
                    new VertexPositionTexture(new Vector3(+1.0f,-1.0f, -1.0f),  new Vector2(1, 1)),
                    new VertexPositionTexture(new Vector3(-1.0f,-1.0f, -1.0f),  new Vector2(0, 1)),
                    // Left                                                               
                    new VertexPositionTexture(new Vector3(-1.0f, +1.0f, -1.0f), new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3(-1.0f, +1.0f, +1.0f), new Vector2(1, 0)),
                    new VertexPositionTexture(new Vector3(-1.0f, -1.0f, +1.0f), new Vector2(1, 1)),
                    new VertexPositionTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(0, 1)),
                    // Right                                                              
                    new VertexPositionTexture(new Vector3(+1.0f, +1.0f, +1.0f), new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3(+1.0f, +1.0f, -1.0f), new Vector2(1, 0)),
                    new VertexPositionTexture(new Vector3(+1.0f, -1.0f, -1.0f), new Vector2(1, 1)),
                    new VertexPositionTexture(new Vector3(+1.0f, -1.0f, +1.0f), new Vector2(0, 1)),
                    // Back                                                               
                    new VertexPositionTexture(new Vector3(+1.0f, +1.0f, -1.0f), new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3(-1.0f, +1.0f, -1.0f), new Vector2(1, 0)),
                    new VertexPositionTexture(new Vector3(-1.0f, -1.0f, -1.0f), new Vector2(1, 1)),
                    new VertexPositionTexture(new Vector3(+1.0f, -1.0f, -1.0f), new Vector2(0, 1)),
                    // Front                                                              
                    new VertexPositionTexture(new Vector3(-1.0f, +1.0f, +1.0f), new Vector2(0, 0)),
                    new VertexPositionTexture(new Vector3(+1.0f, +1.0f, +1.0f), new Vector2(1, 0)),
                    new VertexPositionTexture(new Vector3(+1.0f, -1.0f, +1.0f), new Vector2(1, 1)),
                    new VertexPositionTexture(new Vector3(-1.0f, -1.0f, +1.0f), new Vector2(0, 1)),
                };

            return new Mesh<VertexPositionTexture>(cubeVerticies);
        }
        public static ushort[] generateCubeIndicies_TriangleList_CW()
        {
            return new ushort[]
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23,
            };
        }

        public static Mesh<VertexPositionColour> generateColouredQuad(params RgbaFloat[] colours)
        {
            if(colours.Length < 4)
                throw new ArgumentException("At least 4 colour values are needed for a Quad");

            VertexPositionColour[] quadVerticies = {
                new VertexPositionColour(new Vector2(-1.0f,1.0f),colours[0]),
                new VertexPositionColour(new Vector2(1.0f,1.0f),colours[1]),
                new VertexPositionColour(new Vector2(-1.0f,-1.0f),colours[2]),
                new VertexPositionColour(new Vector2(1.0f,-1.0f),colours[3])
            };

            return new Mesh<VertexPositionColour>(quadVerticies);
        }

        public static Mesh<VertexPositionTexture> generateTexturedQuad()
        {

            VertexPositionTexture[] quadVerticies = {
                new VertexPositionTexture(new Vector3(-1.0f,1.0f,1.0f),new Vector2(0,0)),
                new VertexPositionTexture(new Vector3(1.0f,1.0f,1.0f),new Vector2(1,0)),
                new VertexPositionTexture(new Vector3(-1.0f,-1.0f,1.0f),new Vector2(0,1)),
                new VertexPositionTexture(new Vector3(1.0f,-1.0f,1.0f),new Vector2(1,1))
            };

            return new Mesh<VertexPositionTexture>(quadVerticies);
        }

        public static ushort[] generateQuadIndicies_TriangleStrip_CW()
        {
            return new ushort[]{ 0, 1, 2, 3 };
        }
    }
}