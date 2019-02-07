using System;
using System.Numerics;
using Henzai.Core.Numerics;
using Henzai.Core.Geometry;

namespace Henzai.Core.Acceleration
{
    // https://www.gamedevs.org/uploads/fast-extraction-viewing-frustum-planes-from-world-view-projection-matrix.pdf
    public static class MeshCuller {

        /// <summary>
        /// Culls a <see cref="Henzai.Core.Geometry.GeometryDefinition"/> by testing every triangle of the mesh
        /// </summary>
        public static void FrustumCullGeometryDefinition<T>(ref Matrix4x4 modelViewProjectionMatrix, GeometryDefinition<T> geometryDefinition) where T: struct, VertexLocateable {

            var processedIndicesMap = geometryDefinition.ProcessedIndicesMap;

            processedIndicesMap.Clear();

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

                    if(!processedIndicesMap.ContainsKey(i1)){
                        validVertices[validVertexCounter++] = vertices[i1];
                        processedIndicesMap.Add(i1,true);
                    }
                        validIndices[validIndicesCounter++] = i1;

                    if(!processedIndicesMap.ContainsKey(i2)){
                        validVertices[validVertexCounter++] = vertices[i2];
                        processedIndicesMap.Add(i2,true);
                    }
                        validIndices[validIndicesCounter++] = i2;

                    if(!processedIndicesMap.ContainsKey(i3)){
                        validVertices[validVertexCounter++] = vertices[i3];
                        processedIndicesMap.Add(i3,true);
                    }
                        validIndices[validIndicesCounter++] = i3;
                }
            }

            geometryDefinition.NumberOfValidIndicies = validIndicesCounter;
            geometryDefinition.NumberOfValidVertices = validVertexCounter;   
        }

        //TODO: Use "in" keyword when upgraded to C# >= 7.2
        private static bool IsVertexWithinFrustum(ref Matrix4x4 modelViewProjectionMatrix, ref Vector3 vertex){

            Vector4 frustumRowVector = new Vector4();
            Vector4 vertexHomogeneous = new Vector4(vertex.X,vertex.Y, vertex.Z,1);

            return LeftHalfSpaceCheck(ref modelViewProjectionMatrix, ref frustumRowVector, ref vertexHomogeneous) &&
                RightHalfSpaceCheck(ref modelViewProjectionMatrix, ref frustumRowVector, ref vertexHomogeneous) &&
                TopHalfSpaceCheck(ref modelViewProjectionMatrix, ref frustumRowVector, ref vertexHomogeneous) &&
                BottomHalfSpaceCheck(ref modelViewProjectionMatrix, ref frustumRowVector, ref vertexHomogeneous) &&
                NearHalfSpaceCheck(ref modelViewProjectionMatrix, ref frustumRowVector, ref vertexHomogeneous) &&
                FarHalfSpaceCheck(ref modelViewProjectionMatrix, ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool LeftHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix, ref Vector4 frustumRowVector, ref Vector4 vertexHomogeneous) {
            frustumRowVector.X = modelViewProjectionMatrix.M14 + modelViewProjectionMatrix.M11;
            frustumRowVector.Y = modelViewProjectionMatrix.M24 + modelViewProjectionMatrix.M21;
            frustumRowVector.Z = modelViewProjectionMatrix.M34 + modelViewProjectionMatrix.M31;
            frustumRowVector.W = modelViewProjectionMatrix.M44 + modelViewProjectionMatrix.M41;

            return  0 < Numerics.Geometry.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool RightHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix, ref Vector4 frustumRowVector, ref Vector4 vertexHomogeneous) {
            frustumRowVector.X = modelViewProjectionMatrix.M14 - modelViewProjectionMatrix.M11;
            frustumRowVector.Z = modelViewProjectionMatrix.M34 - modelViewProjectionMatrix.M31;
            frustumRowVector.Y = modelViewProjectionMatrix.M24 - modelViewProjectionMatrix.M21;
            frustumRowVector.W = modelViewProjectionMatrix.M44 - modelViewProjectionMatrix.M41;

            return 0 < Numerics.Geometry.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool TopHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix, ref Vector4 frustumRowVector, ref Vector4 vertexHomogeneous) {
            frustumRowVector.X = modelViewProjectionMatrix.M14 - modelViewProjectionMatrix.M12;
            frustumRowVector.Y = modelViewProjectionMatrix.M24 - modelViewProjectionMatrix.M22;
            frustumRowVector.Z = modelViewProjectionMatrix.M34 - modelViewProjectionMatrix.M32;
            frustumRowVector.W = modelViewProjectionMatrix.M44 - modelViewProjectionMatrix.M42;

            return 0 < Numerics.Geometry.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool BottomHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix, ref Vector4 frustumRowVector, ref Vector4 vertexHomogeneous) {
            frustumRowVector.X = modelViewProjectionMatrix.M14 + modelViewProjectionMatrix.M12;
            frustumRowVector.Y = modelViewProjectionMatrix.M24 + modelViewProjectionMatrix.M22;
            frustumRowVector.Z = modelViewProjectionMatrix.M34 + modelViewProjectionMatrix.M32;
            frustumRowVector.W = modelViewProjectionMatrix.M44 + modelViewProjectionMatrix.M42;

            return 0 < Numerics.Geometry.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool NearHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix, ref Vector4 frustumRowVector, ref Vector4 vertexHomogeneous) {
            frustumRowVector.X = modelViewProjectionMatrix.M31;
            frustumRowVector.Y = modelViewProjectionMatrix.M32;
            frustumRowVector.Z = modelViewProjectionMatrix.M33;
            frustumRowVector.W = modelViewProjectionMatrix.M34;

            return 0 < Numerics.Geometry.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

        private static bool FarHalfSpaceCheck(ref Matrix4x4 modelViewProjectionMatrix, ref Vector4 frustumRowVector, ref Vector4 vertexHomogeneous) {
            frustumRowVector.X = modelViewProjectionMatrix.M14 - modelViewProjectionMatrix.M13;
            frustumRowVector.Y = modelViewProjectionMatrix.M24 - modelViewProjectionMatrix.M23;
            frustumRowVector.Z = modelViewProjectionMatrix.M34 - modelViewProjectionMatrix.M33;
            frustumRowVector.W = modelViewProjectionMatrix.M44 - modelViewProjectionMatrix.M43;

            return 0 < Numerics.Geometry.InMemoryDotProduct(ref frustumRowVector, ref vertexHomogeneous);
        }

    }
}