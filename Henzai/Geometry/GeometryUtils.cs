using System;
using System.Numerics;
using Henzai.Extensions;
using Henzai.Runtime;


namespace Henzai.Geometry
{
    public static class GeometryUtils
    {
        public static void GenerateSphericalTextureCoordinatesFor(Mesh<VertexPositionNormalTexture> mesh, UVMappingTypes mapping){
            var vertices = mesh.vertices;

            for(int i = 0; i < vertices.Length; i++){
                vertices[i].TextureCoordinates = GenerateSphericalCoordiantesfor(vertices[i].Position, mapping);
            }

        }

        public static void GenerateSphericalTextureCoordinatesFor(Mesh<VertexPositionNormalTextureTangent> mesh, UVMappingTypes mapping){
            var vertices = mesh.vertices;

            for(int i = 0; i < vertices.Length; i++){
                vertices[i].TextureCoordinates = GenerateSphericalCoordiantesfor(vertices[i].Position, mapping);
            }

        }

        private static Vector2 GenerateSphericalCoordiantesfor(Vector3 position, UVMappingTypes mapping){
            Vector2 tex;
            double u = 0.0;
            double v = 0.0;
            double x = position.X;
            double y = position.Y;
            double z = position.Z;
            double r 
                = Math.Sqrt(Math.Pow(x,2.0) + Math.Pow(y,2.0) + Math.Pow(z,2.0));

            x /= r;
            y /= r;
            z /= r;


            switch(mapping){
                case UVMappingTypes.Spherical_Coordinates:
                    u = z>= 0 ? Math.Atan2(position.X,position.Z) : Math.Atan2(position.X,-position.Z);
                    v = Math.Acos(position.Y/r);
                break;
                case UVMappingTypes.Central:
                    u = x;
                    v = y;
                break;
                case UVMappingTypes.Stereographic:
                    u = z >= 0 ? x / (1 + z) : x / (1 - z);
                    v = z >= 0 ? y / (1 + z) : y / (1 - z);
                break;

            }

            //TODO: Log to file
            #if DEBUG
                //if(u < 0.0 || u >1.0 || v < 0.0 || v > 1.0)
                    //Console.Error.WriteLine($"Texturecoordinates Out of Range - u:{u}, v:{v}");
            #endif

            tex.X = u.ToFloat();
            tex.Y = v.ToFloat();

            return tex;

        }

        public static void GenerateTangentSpaceFor(Model<VertexPositionNormalTextureTangent> model){

            int numberOfMeshes = model.meshes.Length;
            for(int j = 0; j < numberOfMeshes; j++){
                var vertices = model.meshes[j].vertices;
                var indicies = model.meshes[j].meshIndices;
                var numberOfIndicies = indicies.Length;
                var tangentCountPerVertex = new uint[vertices.Length];
                // Calculate Tangent & Bitangent
                for(int i = 0; i < numberOfIndicies; i+=3){
                    var indicie_0 = indicies[i];
                    var indicie_1 = indicies[i+1];
                    var indicie_2 = indicies[i+2];
                    ref VertexPositionNormalTextureTangent v0 = ref vertices[indicie_0];
                    ref VertexPositionNormalTextureTangent v1 = ref vertices[indicie_1];
                    ref VertexPositionNormalTextureTangent v2 = ref vertices[indicie_2];

                    Vector3 edge1 = v1.Position - v0.Position;
                    Vector3 edge2 = v2.Position - v0.Position;

                    float deltaU1 = v1.TextureCoordinates.X- v0.TextureCoordinates.X;
                    float deltaV1 = v1.TextureCoordinates.Y - v0.TextureCoordinates.Y;
                    float deltaU2 = v2.TextureCoordinates.X - v0.TextureCoordinates.X;
                    float deltaV2 = v2.TextureCoordinates.Y - v0.TextureCoordinates.Y;

                    float inv_det = 1.0f / (deltaU1 * deltaV2 - deltaU2 * deltaV1);

                    Vector3 tangent;
                    tangent.X = inv_det * (deltaV2 * edge1.X - deltaV1 * edge2.X);
                    tangent.Y = inv_det * (deltaV2 * edge1.Y - deltaV1 * edge2.Y);
                    tangent.Z = inv_det * (deltaV2 * edge1.Z - deltaV1 * edge2.Z);

                    tangent = Vector3.Normalize(tangent);

                    v0.Tangent += tangent;
                    v1.Tangent += tangent;
                    v2.Tangent += tangent;

                    tangentCountPerVertex[indicie_0]++;
                    tangentCountPerVertex[indicie_1]++;
                    tangentCountPerVertex[indicie_2]++;

                }

                // Average Tangent + Orthgonalize via Gram-Schmidt
                for(uint i = 0; i < tangentCountPerVertex.Length; i++){
                    uint tangentCount = tangentCountPerVertex[i];
    
                    Vector3 tangent = vertices[i].Tangent;

                    tangent /= tangentCount;
                    tangent = Vector3.Normalize(tangent);

                    Vector3 normal = vertices[i].Normal;

                    vertices[i].Tangent = Vector3.Normalize(tangent - Vector3.Dot(normal,tangent) * normal);

                }
            }
        }

        public static void GenerateTangentAndBitagentSpaceFor(Model<VertexPositionNormalTextureTangentBitangent> model){

            int numberOfMeshes = model.meshes.Length;
            for(int j = 0; j < numberOfMeshes; j++){
                var vertices = model.meshes[j].vertices;
                var indicies = model.meshes[j].meshIndices;
                var numberOfIndicies = indicies.Length;
                var tangentCountPerVertex = new uint[numberOfIndicies];
                var bitangentCountPerVertex = new uint[numberOfIndicies];
                // Calculate Tangent & Bitangent
                for(int i = 0; i < numberOfIndicies; i+=3){
                    var indicie_0 = indicies[i];
                    var indicie_1 = indicies[i+1];
                    var indicie_2 = indicies[i+2];
                    ref VertexPositionNormalTextureTangentBitangent v0 = ref vertices[indicie_0];
                    ref VertexPositionNormalTextureTangentBitangent v1 = ref vertices[indicie_1];
                    ref VertexPositionNormalTextureTangentBitangent v2 = ref vertices[indicie_2];

                    Vector3 edge1 = v1.Position - v0.Position;
                    Vector3 edge2 = v2.Position - v0.Position;

                    Vector2 deltaUV1 = v1.TextureCoordinates - v0.TextureCoordinates;
                    Vector2 deltaUV2 = v2.TextureCoordinates - v0.TextureCoordinates;

                    float inv_det = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

                    Vector3 tangent = inv_det*(edge1*deltaUV2.Y - edge2*deltaUV1.Y);

                    tangent = Vector3.Normalize(tangent);

                    v0.Tangent += tangent;
                    v1.Tangent += tangent;
                    v2.Tangent += tangent;

                    tangentCountPerVertex[indicie_0]++;
                    tangentCountPerVertex[indicie_1]++;
                    tangentCountPerVertex[indicie_2]++;

                    Vector3 bitangent = inv_det*(-deltaUV2.X * edge1 + deltaUV1.X * edge2);

                    bitangent = Vector3.Normalize(bitangent);

                    v0.Bitangent += bitangent;
                    v1.Bitangent += bitangent;
                    v2.Bitangent += bitangent;

                    bitangentCountPerVertex[indicie_0]++;
                    bitangentCountPerVertex[indicie_1]++;
                    bitangentCountPerVertex[indicie_2]++;

                }

                // Average Tangent + Orthgonalize via Gram-Schmidt
                for(uint i = 0; i < numberOfIndicies; i++){
                    uint tangentCount = tangentCountPerVertex[i];
                    if(tangentCount ==0)
                        continue;
    
                    Vector3 tangent = vertices[i].Tangent;

                    tangent /= tangentCount;
                    tangent = Vector3.Normalize(tangent);

                    Vector3 normal = vertices[i].Normal;

                    // vertices[indicie].Tangent = tangent;
                    vertices[i].Tangent = Vector3.Normalize(tangent - Vector3.Dot(normal,tangent) * normal);
                    tangentCountPerVertex[i] = 0;
                }

                // Average Bitangent + Orthgonalize via Gram-Schmidt
                for(uint i = 0; i < numberOfIndicies; i++){
                    uint bitangentCount = bitangentCountPerVertex[i];
                    if(bitangentCount ==0)
                        continue;
    
                    Vector3 bitangent = vertices[i].Bitangent;

                    bitangent /= bitangentCount;
                    bitangent = Vector3.Normalize(bitangent);

                    Vector3 normal = vertices[i].Normal;
                    Vector3 tangent = vertices[i].Tangent;

                    bitangent = Vector3.Normalize(bitangent - Vector3.Dot(normal,bitangent) * normal);
                    bitangent = Vector3.Normalize(bitangent - Vector3.Dot(tangent,bitangent) * tangent);
                    vertices[i].Bitangent = bitangent;

                    bitangentCountPerVertex[i] = 0;
                    
                }
            }
        }

        // http://www.iquilezles.org/www/articles/patchedsphere/patchedsphere.htm
        //TODO: Implement Tangent space calculation for a spehre
    }

}
