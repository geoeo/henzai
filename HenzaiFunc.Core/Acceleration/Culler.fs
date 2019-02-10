module HenzaiFunc.Core.Acceleration.Culler

open System.Numerics
open Henzai.Core.Numerics
open Henzai.Core.Geometry

// https://www.gamedevs.org/uploads/fast-extraction-viewing-frustum-planes-from-world-view-projection-matrix.pdf

let leftHalfSpaceCheck
    (modelViewProjectionMatrix : byref<Matrix4x4>, 
     frustumRowVector : byref<Vector4>, 
     vertexHomogeneous : byref<Vector4>) =
    frustumRowVector.X <- modelViewProjectionMatrix.M14 + modelViewProjectionMatrix.M11
    frustumRowVector.Y <- modelViewProjectionMatrix.M24 + modelViewProjectionMatrix.M21
    frustumRowVector.Z <- modelViewProjectionMatrix.M34 + modelViewProjectionMatrix.M31
    frustumRowVector.W <- modelViewProjectionMatrix.M44 + modelViewProjectionMatrix.M41

    0.0f < Henzai.Core.Numerics.Geometry.InMemoryDotProduct(&frustumRowVector, &vertexHomogeneous)

let rightHalfSpaceCheck
    (modelViewProjectionMatrix : byref<Matrix4x4>, 
     frustumRowVector : byref<Vector4>,
     vertexHomogeneous : byref<Vector4>) =
    frustumRowVector.X <- modelViewProjectionMatrix.M14 - modelViewProjectionMatrix.M11
    frustumRowVector.Z <- modelViewProjectionMatrix.M34 - modelViewProjectionMatrix.M31
    frustumRowVector.Y <- modelViewProjectionMatrix.M24 - modelViewProjectionMatrix.M21
    frustumRowVector.W <- modelViewProjectionMatrix.M44 - modelViewProjectionMatrix.M41

    0.0f < Henzai.Core.Numerics.Geometry.InMemoryDotProduct(&frustumRowVector, &vertexHomogeneous)

let topHalfSpaceCheck
    (modelViewProjectionMatrix : byref<Matrix4x4>, 
     frustumRowVector : byref<Vector4>,
     vertexHomogeneous : byref<Vector4>) =
    frustumRowVector.X <- modelViewProjectionMatrix.M14 - modelViewProjectionMatrix.M12
    frustumRowVector.Y <- modelViewProjectionMatrix.M24 - modelViewProjectionMatrix.M22
    frustumRowVector.Z <- modelViewProjectionMatrix.M34 - modelViewProjectionMatrix.M32
    frustumRowVector.W <- modelViewProjectionMatrix.M44 - modelViewProjectionMatrix.M42

    0.0f < Henzai.Core.Numerics.Geometry.InMemoryDotProduct(&frustumRowVector, &vertexHomogeneous)

let bottomHalfSpaceCheck
    (modelViewProjectionMatrix : byref<Matrix4x4>, 
     frustumRowVector : byref<Vector4>,
     vertexHomogeneous : byref<Vector4>) =
    frustumRowVector.X <- modelViewProjectionMatrix.M14 + modelViewProjectionMatrix.M12
    frustumRowVector.Y <- modelViewProjectionMatrix.M24 + modelViewProjectionMatrix.M22
    frustumRowVector.Z <- modelViewProjectionMatrix.M34 + modelViewProjectionMatrix.M32
    frustumRowVector.W <- modelViewProjectionMatrix.M44 + modelViewProjectionMatrix.M42

    0.0f < Henzai.Core.Numerics.Geometry.InMemoryDotProduct(&frustumRowVector, &vertexHomogeneous)

let nearHalfSpaceCheck
    (modelViewProjectionMatrix : byref<Matrix4x4>, 
     frustumRowVector : byref<Vector4>,
     vertexHomogeneous : byref<Vector4>) =
    frustumRowVector.X <- modelViewProjectionMatrix.M31
    frustumRowVector.Y <- modelViewProjectionMatrix.M32
    frustumRowVector.Z <- modelViewProjectionMatrix.M33
    frustumRowVector.W <- modelViewProjectionMatrix.M34

    0.0f < Henzai.Core.Numerics.Geometry.InMemoryDotProduct(&frustumRowVector, &vertexHomogeneous)

let farHalfSpaceCheck
    (modelViewProjectionMatrix : byref<Matrix4x4>, 
     frustumRowVector : byref<Vector4>,
     vertexHomogeneous : byref<Vector4>) =
    frustumRowVector.X <- modelViewProjectionMatrix.M14 - modelViewProjectionMatrix.M13;
    frustumRowVector.Y <- modelViewProjectionMatrix.M24 - modelViewProjectionMatrix.M23;
    frustumRowVector.Z <- modelViewProjectionMatrix.M34 - modelViewProjectionMatrix.M33;
    frustumRowVector.W <- modelViewProjectionMatrix.M44 - modelViewProjectionMatrix.M43;

    0.0f < Henzai.Core.Numerics.Geometry.InMemoryDotProduct(&frustumRowVector, &vertexHomogeneous)

let IsVertexWithinFrustum(modelViewProjectionMatrix : byref<Matrix4x4>, vertex : byref<Vector3>) =
    let mutable frustumRowVector = Vector4.Zero
    let mutable vertexHomogeneous = Vector4(vertex.X, vertex.Y, vertex.Z, 1.0f)

    leftHalfSpaceCheck(&modelViewProjectionMatrix, &frustumRowVector, &vertexHomogeneous) &&
    rightHalfSpaceCheck(&modelViewProjectionMatrix, &frustumRowVector, &vertexHomogeneous) &&
    topHalfSpaceCheck(&modelViewProjectionMatrix, &frustumRowVector, &vertexHomogeneous) &&
    bottomHalfSpaceCheck(&modelViewProjectionMatrix, &frustumRowVector, &vertexHomogeneous) &&
    nearHalfSpaceCheck(&modelViewProjectionMatrix, &frustumRowVector, &vertexHomogeneous) &&
    farHalfSpaceCheck(&modelViewProjectionMatrix, &frustumRowVector, &vertexHomogeneous)

/// <summary>
/// Culls a <see cref="Henzai.Core.Geometry.GeometryDefinition"/> by testing every triangle of the mesh
/// </summary>
let FrustumCullGeometryDefinition(modelViewProjectionMatrix : byref<Matrix4x4>, geometryDefinition : Henzai.Core.Geometry.GeometryDefinition<'T>) =
    let processedIndicesMap = geometryDefinition.ProcessedIndicesMap;
    processedIndicesMap.Clear()

    let vertices = geometryDefinition.GetVertices
    let indices = geometryDefinition.GetIndices
    let validVertices = geometryDefinition.GetValidVertices
    let validIndices = geometryDefinition.GetValidIndices

    let mutable validIndicesCounter = 0
    let mutable validVertexCounter = 0

    for i in 0 .. 3 .. indices.Length-1 do
        let i1 = indices.[i]
        let i2 = indices.[i+1]
        let i3 = indices.[i+2]

        let v1 = vertices.[int i1].GetPosition()
        let v2 = vertices.[int i2].GetPosition()
        let v3 = vertices.[int i3].GetPosition()

        // If at least one vertex is within the frustum, the triangle is not culled.
        if(IsVertexWithinFrustum(&modelViewProjectionMatrix, &v1) ||
           IsVertexWithinFrustum(&modelViewProjectionMatrix, &v2) ||
           IsVertexWithinFrustum(&modelViewProjectionMatrix, &v3)) then

           if(not(processedIndicesMap.ContainsKey(i1))) then
            validVertices.[validVertexCounter] <- vertices.[int i1]
            validVertexCounter <- validVertexCounter+1
            processedIndicesMap.Add(i1, true)
           validIndices.[validIndicesCounter] <- i1
           validIndicesCounter <- validIndicesCounter+1

           if(not(processedIndicesMap.ContainsKey(i2))) then
            validVertices.[validVertexCounter] <- vertices.[int i2]
            validVertexCounter <- validVertexCounter+1
            processedIndicesMap.Add(i2, true)
           validIndices.[validIndicesCounter] <- i2
           validIndicesCounter <- validIndicesCounter+1

           if(not(processedIndicesMap.ContainsKey(i3))) then
            validVertices.[validVertexCounter] <- vertices.[int i3]
            validVertexCounter <- validVertexCounter+1
            processedIndicesMap.Add(i3, true)
           validIndices.[validIndicesCounter] <- i3
           validIndicesCounter <- validIndicesCounter+1

    geometryDefinition.NumberOfValidIndicies <- validIndicesCounter
    geometryDefinition.NumberOfValidVertices <- validVertexCounter

    ()