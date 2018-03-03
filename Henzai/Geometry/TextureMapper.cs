using System;
using System.Numerics;
using Henzai.Extensions;


namespace Henzai.Geometry
{
    public static class TextureMapper
    {
        public static void GenerateSphericalTextureCoordinatesFor(Mesh<VertexPositionNormalTexture> mesh){
            var vertices = mesh.vertices;

            for(int i = 0; i < vertices.Length; i++){
                vertices[i].TextureCoordinates = GenerateSphericalCoordiantesfor(vertices[i].Position);
            }

        }

        // http://mathworld.wolfram.com/SphericalCoordinates.html
        private static Vector2 GenerateSphericalCoordiantesfor(Vector3 position){
            Vector2 tex;
            double r 
                = Math.Sqrt(Math.Pow(position.X,2.0) + Math.Pow(position.Y,2.0) + Math.Pow(position.Z,2.0));

            double theta = Math.Atan2(position.Y,position.X); // [-Pi,Pi]
            double phi = Math.Acos(position.Z.ToDouble()/r); // [0,Pi]

            double u = (theta/Math.PI + 1.0)/2.0;
            double v = phi / Math.PI;

            #if DEBUG
                if(u < 0.0 || u >1.0 || v < 0.0 || v > 1.0)
                    throw new ArithmeticException($"Texturecoordinates Out of Range - u:{u}, v:{v}");
            #endif

            tex.X = u.ToFloat();
            tex.Y = v.ToFloat();

            return tex;

        }
    }

}