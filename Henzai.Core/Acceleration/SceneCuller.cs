using System;
using System.Numerics;
using Henzai.Core.Numerics;
using Henzai.Core.Geometry;

namespace Henzai.Core.Acceleration
{
    public static class SceneCuller {

        private static Vector4 frustumRowVector = new Vector4();
        private static Vector4 vertexHomogeneous = new Vector4(0,0,0,1);

        public static void FrustumCullGeometryDefinition<T>(ref Matrix4x4 viewProjectionMatrix, GeometryDefinition<T> geometryDefinition) where T: struct, VertexLocateable{
            
        }

        private static bool IsVertexWithinFrustum(ref Matrix4x4 viewProjectionMatrix, ref Vector3 vertex){
            vertexHomogeneous.X = vertex.X;
            vertexHomogeneous.Y = vertex.Y;
            vertexHomogeneous.Z = vertex.Z;

            return IsVertexWithinLeftHalfSpace(ref viewProjectionMatrix);
        }

        private static bool IsVertexWithinLeftHalfSpace(ref Matrix4x4 viewProjectionMatrix) {
            frustumRowVector.X = -(viewProjectionMatrix.M31 + viewProjectionMatrix.M11);
            frustumRowVector.Y = -(viewProjectionMatrix.M32 + viewProjectionMatrix.M12);
            frustumRowVector.Z = -(viewProjectionMatrix.M33 + viewProjectionMatrix.M13);
            frustumRowVector.W = -(viewProjectionMatrix.M34 + viewProjectionMatrix.M14);

            return GeometryUtils.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous) >= 0;
        }

    }
}