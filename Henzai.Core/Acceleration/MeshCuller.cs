using System;
using System.Numerics;
using System.Collections.Generic;
using Henzai.Core.Numerics;
using Henzai.Core.Geometry;

namespace Henzai.Core.Acceleration
{
    // https://www.gamedevs.org/uploads/fast-extraction-viewing-frustum-planes-from-world-view-projection-matrix.pdf
    public static class MeshCuller {

        private static Vector4 _frustumRowVector = new Vector4();
        private static Vector4 _vertexHomogeneous = new Vector4(0,0,0,1);
        private static Dictionary<ushort, bool> _processedIndicesMap = new Dictionary<ushort, bool>();

        public static void FrustumCullGeometryDefinition<T>(ref Matrix4x4 viewProjectionMatrix, GeometryDefinition<T> geometryDefinition) where T: struct, VertexLocateable {

            _processedIndicesMap.Clear();

            T[] vertices = geometryDefinition.GetVertices;
            ushort[] indices = geometryDefinition.GetIndices;

            T[] validVertices = geometryDefinition.GetValidVertices;
            ushort[] validIndices = geometryDefinition.GetValidIndices;

            int validIndicesCounter = 0;
            int validVertexCounter = 0;

            for(int i = 0; i < indices.Length; i+=3){
                ushort i1 = indices[i];
                ushort i2 = indices[i+1];
                ushort i3 = indices[i+2];

                Vector3 v1 = vertices[i1].GetPosition();
                Vector3 v2 = vertices[i2].GetPosition();
                Vector3 v3 = vertices[i3].GetPosition();

                // If at least one vertex is within the frustum, the triangle is not culled.
                if(IsVertexWithinFrustum(ref viewProjectionMatrix,ref v1) 
                || IsVertexWithinFrustum(ref viewProjectionMatrix,ref v2)
                || IsVertexWithinFrustum(ref viewProjectionMatrix,ref v3)){

                    if(!_processedIndicesMap.ContainsKey(i1)){
                        validIndices[validIndicesCounter++] = i1;
                        _processedIndicesMap.Add(i1,true);
                    }
                    validVertices[validVertexCounter++] = vertices[i1];

                    if(!_processedIndicesMap.ContainsKey(i2)){
                        validIndices[validIndicesCounter++] = i2;
                        _processedIndicesMap.Add(i2,true);
                    }
                    validVertices[validVertexCounter++] = vertices[i2];

                    if(!_processedIndicesMap.ContainsKey(i3)){
                        validIndices[validIndicesCounter++] = i3;
                        _processedIndicesMap.Add(i3,true);
                    }
                    validVertices[validVertexCounter++] = vertices[i3];
                }
            }

            geometryDefinition.NumberOfValidIndicies = validIndicesCounter;
            geometryDefinition.NumberOfValidVertices = validVertexCounter;   
        }

        private static bool IsVertexWithinFrustum(ref Matrix4x4 viewProjectionMatrix, ref Vector3 vertex){
            _vertexHomogeneous.X = vertex.X;
            _vertexHomogeneous.Y = vertex.Y;
            _vertexHomogeneous.Z = vertex.Z;

            return LeftHalfSpaceCheck(ref viewProjectionMatrix) &&
                RightHalfSpaceCheck(ref viewProjectionMatrix) &&
                TopHalfSpaceCheck(ref viewProjectionMatrix) &&
                BottomHalfSpaceCheck(ref viewProjectionMatrix) &&
                NearHalfSpaceCheck(ref viewProjectionMatrix) &&
                FarHalfSpaceCheck(ref viewProjectionMatrix);
        }

        private static bool LeftHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            _frustumRowVector.X = viewProjectionMatrix.M41 + viewProjectionMatrix.M11;
            _frustumRowVector.Y = viewProjectionMatrix.M42 + viewProjectionMatrix.M12;
            _frustumRowVector.Z = viewProjectionMatrix.M43 + viewProjectionMatrix.M13;
            _frustumRowVector.W = viewProjectionMatrix.M44 + viewProjectionMatrix.M14;

            return  0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool RightHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            _frustumRowVector.X = viewProjectionMatrix.M41 - viewProjectionMatrix.M11;
            _frustumRowVector.Y = viewProjectionMatrix.M42 - viewProjectionMatrix.M12;
            _frustumRowVector.Z = viewProjectionMatrix.M43 - viewProjectionMatrix.M13;
            _frustumRowVector.W = viewProjectionMatrix.M44 - viewProjectionMatrix.M14;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool TopHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            _frustumRowVector.X = viewProjectionMatrix.M41 - viewProjectionMatrix.M21;
            _frustumRowVector.Y = viewProjectionMatrix.M42 - viewProjectionMatrix.M22;
            _frustumRowVector.Z = viewProjectionMatrix.M43 - viewProjectionMatrix.M23;
            _frustumRowVector.W = viewProjectionMatrix.M44 - viewProjectionMatrix.M24;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool BottomHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            _frustumRowVector.X = viewProjectionMatrix.M41 + viewProjectionMatrix.M21;
            _frustumRowVector.Y = viewProjectionMatrix.M42 + viewProjectionMatrix.M22;
            _frustumRowVector.Z = viewProjectionMatrix.M43 + viewProjectionMatrix.M23;
            _frustumRowVector.W = viewProjectionMatrix.M44 + viewProjectionMatrix.M24;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool NearHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            _frustumRowVector.X = viewProjectionMatrix.M31;
            _frustumRowVector.Y = viewProjectionMatrix.M32;
            _frustumRowVector.Z = viewProjectionMatrix.M33;
            _frustumRowVector.W = viewProjectionMatrix.M34;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool FarHalfSpaceCheck(ref Matrix4x4 viewProjectionMatrix) {
            _frustumRowVector.X = viewProjectionMatrix.M41 - viewProjectionMatrix.M31;
            _frustumRowVector.Y = viewProjectionMatrix.M42 - viewProjectionMatrix.M32;
            _frustumRowVector.Z = viewProjectionMatrix.M43 - viewProjectionMatrix.M33;
            _frustumRowVector.W = viewProjectionMatrix.M44 - viewProjectionMatrix.M34;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

    }
}