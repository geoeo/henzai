using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Henzai.Extensions;
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

        public static Mesh<VertexPositionNDCColour> generateColouredQuad(params RgbaFloat[] colours)
        {
            if(colours.Length < 4)
                throw new ArgumentException("At least 4 colour values are needed for a Quad");

            VertexPositionNDCColour[] quadVerticies = {
                new VertexPositionNDCColour(new Vector2(-1.0f,1.0f),colours[0]),
                new VertexPositionNDCColour(new Vector2(1.0f,1.0f),colours[1]),
                new VertexPositionNDCColour(new Vector2(-1.0f,-1.0f),colours[2]),
                new VertexPositionNDCColour(new Vector2(1.0f,-1.0f),colours[3])
            };

            return new Mesh<VertexPositionNDCColour>(quadVerticies);
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

        // TODO: Refactor so that it works with generic types
        // https://gamedev.stackexchange.com/questions/150191/opengl-calculate-uv-sphere-vertices
        public static Mesh<VertexPositionNormalTextureTangent> generateSphere(int numLatitudeLines, int numLongitudeLines, float radius){
            // One vertex at every latitude-longitude intersection,
            // plus one for the north pole and one for the south.
            // One meridian serves as a UV seam, so we double the vertices there.
            int numVertices = numLatitudeLines * (numLongitudeLines + 1) + 2;

            VertexPositionNormalTextureTangent[] vertices = new VertexPositionNormalTextureTangent[numVertices];
            List<uint> indices = new List<uint>();

            // North pole.
            vertices[0].Position = new Vector3(0, radius, 0);
            vertices[0].TextureCoordinates = new Vector2(0, 0);

            // South pole.
            vertices[numVertices - 1].Position = new Vector3(0, -radius, 0);
            vertices[numVertices - 1].TextureCoordinates = new Vector2(0, 1);

            // +1.0f because there's a gap between the poles and the first parallel.
            float latitudeSpacing = 1.0f / (numLatitudeLines + 1.0f);
            float longitudeSpacing = 1.0f / (numLongitudeLines);

            // start writing new vertices at position 1
            int v = 1;
            for(int latitude = 0; latitude < numLatitudeLines; latitude++) {
                for(int longitude = 0; longitude <= numLongitudeLines; longitude++) {

                // Scale coordinates into the 0...1 texture coordinate range,
                // with north at the top (y = 0).
                vertices[v].TextureCoordinates = new Vector2(
                                    longitude.ToFloat() * longitudeSpacing,
                                    (latitude.ToFloat() + 1.0f) * latitudeSpacing                            
                                );

                // Convert to spherical coordinates:
                // theta is a longitude angle (around the equator) in radians.
                // phi is a latitude angle (north or south of the equator).
                float theta =  vertices[v].TextureCoordinates.X * 2.0f * Math.PI.ToFloat();
                float phi = vertices[v].TextureCoordinates.Y * Math.PI.ToFloat();

                // This determines the radius of the ring of this line of latitude.
                // It's widest at the equator, and narrows as phi increases/decreases.
                // float c = Math.Sin(phi).ToFloat();

                // Usual formula for a vector in spherical coordinates.
                // You can exchange x & z to wind the opposite way around the sphere.
                 vertices[v].Position = new Vector3(
                    Math.Sin(phi).ToFloat() * Math.Sin(theta).ToFloat(),
                    Math.Cos(phi).ToFloat(),
                    Math.Sin(phi).ToFloat() * Math.Cos(theta).ToFloat()
                                ) * radius;

                vertices[v].Normal = Vector3.Normalize(vertices[v].Position);

                if(latitude < numLatitudeLines -1){
                                        indices.Add(v.ToUnsigned());
                    indices.Add(v.ToUnsigned()+1.ToUnsigned());
                    indices.Add(v.ToUnsigned()+1.ToUnsigned()+numLongitudeLines.ToUnsigned());
                    
                    indices.Add(v.ToUnsigned()+1.ToUnsigned());
                    indices.Add(v.ToUnsigned()+2.ToUnsigned()+numLongitudeLines.ToUnsigned());
                    indices.Add(v.ToUnsigned()+numLongitudeLines.ToUnsigned()+1.ToUnsigned());
                }

                // Proceed to the next vertex.
                v++;
                }
            }

            // North pole indices
            for(int longitude = 1; longitude < numLongitudeLines; longitude++) {
                indices.Add(0);
                indices.Add(longitude.ToUnsigned()+1.ToUnsigned());
                indices.Add(longitude.ToUnsigned());
            }
             indices.Add(0);
             indices.Add(1);
             indices.Add(numLongitudeLines.ToUnsigned());

            v-= numLongitudeLines+1;
            // southpole
            for(int longitude = 0; longitude <= numLongitudeLines; longitude++) {
                indices.Add(v.ToUnsigned() + longitude.ToUnsigned());
                indices.Add(v.ToUnsigned()+ longitude.ToUnsigned() +1.ToUnsigned());
                indices.Add(numVertices.ToUnsigned()-1.ToUnsigned());
            }
            indices.Add(numVertices.ToUnsigned()-2.ToUnsigned());
            indices.Add(v.ToUnsigned()+1.ToUnsigned());
            indices.Add(numVertices.ToUnsigned()-1.ToUnsigned());
            return new Mesh<VertexPositionNormalTextureTangent>(vertices,indices.ToArray());
        }
    }
}