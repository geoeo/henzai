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

        /// <summary>
        /// Culls a <see cref="Henzai.Core.Geometry.GeometryDefinition"/> by testing every triangle of the mesh
        /// </summary>
        public static void FrustumCullGeometryDefinition<T>(ref Matrix4x4 modelViewProjectionMatrix, GeometryDefinition<T> geometryDefinition) where T: struct, VertexLocateable {

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
                if(IsVertexWithinFrustum(ref modelViewProjectionMatrix,ref v1) 
                || IsVertexWithinFrustum(ref modelViewProjectionMatrix,ref v2)
                || IsVertexWithinFrustum(ref modelViewProjectionMatrix,ref v3)){

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

        private static bool IsVertexWithinFrustum(ref Matrix4x4 modelViewProjectionMatrix, ref Vector3 vertex){
            _vertexHomogeneous.X = vertex.X;
            _vertexHomogeneous.Y = vertex.Y;
            _vertexHomogeneous.Z = vertex.Z;

            return LeftHalfSpaceCheck(ref modelViewProjectionMatrix) &&
                RightHalfSpaceCheck(ref modelViewProjectionMatrix) &&
                TopHalfSpaceCheck(ref modelViewProjectionMatrix) &&
                BottomHalfSpaceCheck(ref modelViewProjectionMatrix) &&
                NearHalfSpaceCheck(ref modelViewProjectionMatrix) &&
                FarHalfSpaceCheck(ref modelViewProjectionMatrix);
        }

        private static bool LeftHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix) {
            _frustumRowVector.X = modelViewProjectionMatrix.M14 + modelViewProjectionMatrix.M11;
            _frustumRowVector.Y = modelViewProjectionMatrix.M24 + modelViewProjectionMatrix.M21;
            _frustumRowVector.Z = modelViewProjectionMatrix.M34 + modelViewProjectionMatrix.M31;
            _frustumRowVector.W = modelViewProjectionMatrix.M44 + modelViewProjectionMatrix.M41;

            return  0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool RightHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix) {
            _frustumRowVector.X = modelViewProjectionMatrix.M14 - modelViewProjectionMatrix.M11;
            _frustumRowVector.Y = modelViewProjectionMatrix.M24 - modelViewProjectionMatrix.M21;
            _frustumRowVector.Z = modelViewProjectionMatrix.M34 - modelViewProjectionMatrix.M31;
            _frustumRowVector.W = modelViewProjectionMatrix.M44 - modelViewProjectionMatrix.M41;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool TopHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix) {
            _frustumRowVector.X = modelViewProjectionMatrix.M14 - modelViewProjectionMatrix.M12;
            _frustumRowVector.Y = modelViewProjectionMatrix.M24 - modelViewProjectionMatrix.M22;
            _frustumRowVector.Z = modelViewProjectionMatrix.M34 - modelViewProjectionMatrix.M32;
            _frustumRowVector.W = modelViewProjectionMatrix.M44 - modelViewProjectionMatrix.M42;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool BottomHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix) {
            _frustumRowVector.X = modelViewProjectionMatrix.M14 + modelViewProjectionMatrix.M12;
            _frustumRowVector.Y = modelViewProjectionMatrix.M24 + modelViewProjectionMatrix.M22;
            _frustumRowVector.Z = modelViewProjectionMatrix.M34 + modelViewProjectionMatrix.M32;
            _frustumRowVector.W = modelViewProjectionMatrix.M44 + modelViewProjectionMatrix.M42;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool NearHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix) {
            _frustumRowVector.X = modelViewProjectionMatrix.M31;
            _frustumRowVector.Y = modelViewProjectionMatrix.M32;
            _frustumRowVector.Z = modelViewProjectionMatrix.M33;
            _frustumRowVector.W = modelViewProjectionMatrix.M34;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

        private static bool FarHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix) {
            _frustumRowVector.X = modelViewProjectionMatrix.M14 - modelViewProjectionMatrix.M13;
            _frustumRowVector.Y = modelViewProjectionMatrix.M24 - modelViewProjectionMatrix.M23;
            _frustumRowVector.Z = modelViewProjectionMatrix.M34 - modelViewProjectionMatrix.M33;
            _frustumRowVector.W = modelViewProjectionMatrix.M44 - modelViewProjectionMatrix.M43;

            return 0 < GeometryUtils.InMemoryDotProduct(ref _frustumRowVector, ref _vertexHomogeneous);
        }

    }
}