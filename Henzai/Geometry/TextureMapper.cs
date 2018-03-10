using System;
using System.Numerics;
using Henzai.Extensions;
using Henzai.Runtime;


namespace Henzai.Geometry
{
    //TODO: Refactor to work with generic structs
    public static class TextureMapper
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

            #if DEBUG
                if(u < 0.0 || u >1.0 || v < 0.0 || v > 1.0)
                    Console.Error.WriteLine($"Texturecoordinates Out of Range - u:{u}, v:{v}");
            #endif

            tex.X = u.ToFloat();
            tex.Y = v.ToFloat();

            return tex;

        }

        //TODO: Implement Tangent space calculation with Gram-Schmidt (for loaded models)
        public static void GenerateTangentSpaceFor(Model<VertexPositionNormalTextureTangent> model){

            int numberOfMeshes = model.meshes.Length;
            for(int j = 0; j < numberOfMeshes; j++){
                var vertices = model.meshes[j].vertices;
                var indicies = model.meshIndicies[j];
                var numberOfIndicies = indicies.Length;
                var tangentCountPerVertex = new uint[vertices.Length];
                // Calculate Tangent
                for(int i = 0; i < numberOfIndicies; i+=3){
                    var indicie_0 = indicies[i];
                    var indicie_1 = indicies[i+1];
                    var indicie_2 = indicies[i+2];
                    var v0 = vertices[indicie_0];
                    var v1 = vertices[indicie_1];
                    var v2 = vertices[indicie_2];

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

                    v0.Tangent += tangent;
                    v1.Tangent += tangent;
                    v2.Tangent += tangent;

                    tangentCountPerVertex[indicie_0]++;
                    tangentCountPerVertex[indicie_1]++;
                    tangentCountPerVertex[indicie_2]++;

                }

                // Average Tangent + Orthgonalize via Gram-Schmidt
                for(int i = 0; i < numberOfIndicies; i++){
                    uint indicie = indicies[i];
                    uint tangentCount = tangentCountPerVertex[indicie];
                    if (tangentCount == 0)
                        continue;
                    Vector3 tangent = vertices[indicie].Tangent;

                    tangent /= tangentCount;
                    vertices[indicie].Tangent = Vector3.Normalize(tangent);

                    tangent = vertices[indicie].Tangent;
                    Vector3 normal = vertices[indicie].Normal;

                    vertices[indicie].Tangent = Vector3.Normalize(tangent - Vector3.Dot(normal,tangent) * normal);

                    tangentCountPerVertex[indicie] = 0;
                }



            }


        }

        // http://www.iquilezles.org/www/articles/patchedsphere/patchedsphere.htm
        //TODO: Implement Tangent space calculation for a spehre
    }

}
