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

        public static void FrustumCullGeometryDefinition<T>(ref Matrix4x4 worldViewProjectionMatrix, GeometryDefinition<T> geometryDefinition) where T: struct, VertexLocateable {

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
                if(IsVertexWithinFrustum(ref worldViewProjectionMatrix,ref v1) 
                || IsVertexWithinFrustum(ref worldViewProjectionMatrix,ref v2)
                || IsVertexWithinFrustum(ref worldViewProjectionMatrix,ref v3)){

                    if(!_processedIndicesMap.ContainsKey(i1)){
                        validVertices[validVertexCounter++] = vertices[i1];
                        _processedIndicesMap.Add(i1,true);
                    }
                        validIndices[validIndicesCounter++] = i1;

                    if(!_processedIndicesMap.ContainsKey(i2)){
                        validVertices[validVertexCounter++] = vertices[i2];
                        _processedIndicesMap.Add(i2,true);
                    }
                        validIndices[validIndicesCounter++] = i2;

                    if(!_processedIndicesMap.ContainsKey(i3)){
                        validVertices[validVertexCounter++] = vertices[i3];
                        _processedIndicesMap.Add(i3,true);
                    }
                        validIndices[validIndicesCounter++] = i3;
                }
            }

            geometryDefinition.NumberOfValidIndicies = validIndicesCounter;
            geometryDefinition.NumberOfValidVertices = validVertexCounter;   
        }

        public static bool IsGeometryDefinitionCulled<T>(ref Matrix4x4 worldViewProjectionMatrix, GeometryDefinition<T> geometryDefinition) where T: struct, VertexLocateable {
            T[] vertices = geometryDefinition.GetVertices;
            ushort[] indices = geometryDefinition.GetIndices;

            bool isCulled = true;

            for(int i = 0; i < indices.Length; i+=3){
                ushort i1 = indices[i];
                ushort i2 = indices[i+1];
                ushort i3 = indices[i+2];

                Vector3 v1 = vertices[i1].GetPosition();
                Vector3 v2 = vertices[i2].GetPosition();
                Vector3 v3 = vertices[i3].GetPosition();

                // If at least one vertex is within the frustum, the triangle is not culled.
                if(IsVertexWithinFrustum(ref worldViewProjectionMatrix,ref v1) 
                || IsVertexWithinFrustum(ref worldViewProjectionMatrix,ref v2)
                || IsVertexWithinFrustum(ref worldViewProjectionMatrix,ref v3)){
                    isCulled = false;
                    break;
                }
            }

            return isCulled; 
        }

        private static bool IsVertexWithinFrustum(ref Matrix4x4 worldViewProjectionMatrix, ref Vector3 vertex){
            _vertexHomogeneous.X = vertex.X;
            _vertexHomogeneous.Y = vertex.Y;
            _vertexHomogeneous.Z = vertex.Z;

            return LeftHalfSpaceCheck(ref worldViewProjectionMatrix) &&
                RightHalfSpaceCheck(ref worldViewProjectionMatrix) &&
                TopHalfSpaceCheck(ref worldViewProjectionMatrix) &&
                BottomHalfSpaceCheck(ref worldViewProjectionMatrix) &&
                NearHalfSpaceCheck(ref worldViewProjectionMatrix) &&
                FarHalfSpaceCheck(ref worldViewProjectionMatrix);
        }

        private static bool LeftHalfSpaceCheck(ref Matrix4x4 worldViewProjectionMatrix) {
            _frustumRowVector.X = worldViewProjectionMatrix.M14 + worldViewProjectionMatrix.M11;
            _frustumRowVector.Y = worldViewProjectionMatrix.M24 + worldViewProjectionMatrix.M21;
            _frustumRowVector.Z = worldViewProjectionMatrix.M34 + worldViewProjectionMatrix.M31;
            _frustumRowVector.W = worldViewProjectionMatrix.M44 + worldViewProjectionMatrix.M41;

            return  0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool RightHalfSpaceCheck(ref Matrix4x4 worldViewProjectionMatrix) {
            _frustumRowVector.X = worldViewProjectionMatrix.M14 - worldViewProjectionMatrix.M11;
            _frustumRowVector.Y = worldViewProjectionMatrix.M24 - worldViewProjectionMatrix.M21;
            _frustumRowVector.Z = worldViewProjectionMatrix.M34 - worldViewProjectionMatrix.M31;
            _frustumRowVector.W = worldViewProjectionMatrix.M44 - worldViewProjectionMatrix.M41;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool TopHalfSpaceCheck(ref Matrix4x4 worldViewProjectionMatrix) {
            _frustumRowVector.X = worldViewProjectionMatrix.M14 - worldViewProjectionMatrix.M12;
            _frustumRowVector.Y = worldViewProjectionMatrix.M24 - worldViewProjectionMatrix.M22;
            _frustumRowVector.Z = worldViewProjectionMatrix.M34 - worldViewProjectionMatrix.M32;
            _frustumRowVector.W = worldViewProjectionMatrix.M44 - worldViewProjectionMatrix.M42;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool BottomHalfSpaceCheck(ref Matrix4x4 worldViewProjectionMatrix) {
            _frustumRowVector.X = worldViewProjectionMatrix.M14 + worldViewProjectionMatrix.M12;
            _frustumRowVector.Y = worldViewProjectionMatrix.M24 + worldViewProjectionMatrix.M22;
            _frustumRowVector.Z = worldViewProjectionMatrix.M34 + worldViewProjectionMatrix.M32;
            _frustumRowVector.W = worldViewProjectionMatrix.M44 + worldViewProjectionMatrix.M42;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool NearHalfSpaceCheck(ref Matrix4x4 worldViewProjectionMatrix) {
            _frustumRowVector.X = worldViewProjectionMatrix.M31;
            _frustumRowVector.Y = worldViewProjectionMatrix.M32;
            _frustumRowVector.Z = worldViewProjectionMatrix.M33;
            _frustumRowVector.W = worldViewProjectionMatrix.M34;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool FarHalfSpaceCheck(ref Matrix4x4 worldViewProjectionMatrix) {
            _frustumRowVector.X = worldViewProjectionMatrix.M14 - worldViewProjectionMatrix.M13;
            _frustumRowVector.Y = worldViewProjectionMatrix.M24 - worldViewProjectionMatrix.M23;
            _frustumRowVector.Z = worldViewProjectionMatrix.M34 - worldViewProjectionMatrix.M33;
            _frustumRowVector.W = worldViewProjectionMatrix.M44 - worldViewProjectionMatrix.M43;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

    }
}