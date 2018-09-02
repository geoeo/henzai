using System;
using System.Numerics;
using Henzai.Core.Numerics;
using Henzai.Core.Geometry;

namespace Henzai.Core.Acceleration
{
    // https://www.gamedevs.org/uploads/fast-extraction-viewing-frustum-planes-from-world-view-projection-matrix.pdf
    public static class SceneCuller {

        private static Vector4 frustumRowVector = new Vector4();
        private static Vector4 vertexHomogeneous = new Vector4(0,0,0,1);

        public static void FrustumCullGeometryDefinition<T>(ref Matrix4x4 viewProjectionMatrix, GeometryDefinition<T> geometryDefinition) where T: struct, VertexLocateable{
            
        }

        private static bool IsVertexWithinFrustum(ref Matrix4x4 viewProjectionMatrix, ref Vector3 vertex){
            vertexHomogeneous.X = vertex.X;
            vertexHomogeneous.Y = vertex.Y;
            vertexHomogeneous.Z = vertex.Z;

            return LeftHalfSpaceCheck(ref viewProjectionMatrix) &&
                RightHalfSpaceCheck(ref viewProjectionMatrix) &&
                TopHalfSpaceCheck(ref viewProjectionMatrix) &&
                BottomHalfSpaceCheck(ref viewProjectionMatrix) &&
                NearHalfSpaceCheck(ref viewProjectionMatrix) &&
                FarHalfSpaceCheck(ref viewProjectionMatrix);
        }

        private static bool LeftHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            frustumRowVector.X = viewProjectionMatrix.M41 + viewProjectionMatrix.M11;
            frustumRowVector.Y = viewProjectionMatrix.M42 + viewProjectionMatrix.M12;
            frustumRowVector.Z = viewProjectionMatrix.M43 + viewProjectionMatrix.M13;
            frustumRowVector.W = viewProjectionMatrix.M44 + viewProjectionMatrix.M14;

            return  0 < GeometryUtils.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool RightHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            frustumRowVector.X = viewProjectionMatrix.M41 - viewProjectionMatrix.M11;
            frustumRowVector.Y = viewProjectionMatrix.M42 - viewProjectionMatrix.M12;
            frustumRowVector.Z = viewProjectionMatrix.M43 - viewProjectionMatrix.M13;
            frustumRowVector.W = viewProjectionMatrix.M44 - viewProjectionMatrix.M14;

            return 0 < GeometryUtils.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool TopHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            frustumRowVector.X = viewProjectionMatrix.M41 - viewProjectionMatrix.M21;
            frustumRowVector.Y = viewProjectionMatrix.M42 - viewProjectionMatrix.M22;
            frustumRowVector.Z = viewProjectionMatrix.M43 - viewProjectionMatrix.M23;
            frustumRowVector.W = viewProjectionMatrix.M44 - viewProjectionMatrix.M24;

            return 0 < GeometryUtils.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool BottomHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            frustumRowVector.X = viewProjectionMatrix.M41 + viewProjectionMatrix.M21;
            frustumRowVector.Y = viewProjectionMatrix.M42 + viewProjectionMatrix.M22;
            frustumRowVector.Z = viewProjectionMatrix.M43 + viewProjectionMatrix.M23;
            frustumRowVector.W = viewProjectionMatrix.M44 + viewProjectionMatrix.M24;

            return 0 < GeometryUtils.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool NearHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            frustumRowVector.X = viewProjectionMatrix.M31;
            frustumRowVector.Y = viewProjectionMatrix.M32;
            frustumRowVector.Z = viewProjectionMatrix.M33;
            frustumRowVector.W = viewProjectionMatrix.M34;

            return 0 < GeometryUtils.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool FarHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            frustumRowVector.X = viewProjectionMatrix.M41 - viewProjectionMatrix.M31;
            frustumRowVector.Y = viewProjectionMatrix.M42 - viewProjectionMatrix.M32;
            frustumRowVector.Z = viewProjectionMatrix.M43 - viewProjectionMatrix.M33;
            frustumRowVector.W = viewProjectionMatrix.M44 - viewProjectionMatrix.M34;

            return 0 < GeometryUtils.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

    }
}