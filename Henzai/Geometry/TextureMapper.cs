using System;
using System.Numerics;
using Henzai.Extensions;
using Henzai.Runtime;


namespace Henzai.Geometry
{
    public static class TextureMapper
    {
        public static void GenerateSphericalTextureCoordinatesFor(Mesh<VertexPositionNormalTexture> mesh, UVMappingTypes mapping){
            var vertices = mesh.vertices;

            for(int i = 0; i < vertices.Length; i++){
                vertices[i].TextureCoordinates = GenerateSphericalCoordiantesfor(vertices[i].Position, mapping);
            }

        }

        // http://www.iquilezles.org/www/articles/patchedsphere/patchedsphere.htm
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

        //TODO: Implement Tangent space calculation for a spehre
    }

}
