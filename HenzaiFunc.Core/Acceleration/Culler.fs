namespace HenzaiFunc.Core.Acceleration

open System.Numerics
open System.Collections.Generic;
open Henzai.Core.Numerics
open Henzai.Core.VertexGeometry
open System.Collections.Generic

module Culler =
    
    let leftHalfSpaceCheck
        (modelViewProjectionMatrix : byref<Matrix4x4>) 
        (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        Geometry.ExtractLeftPlane(&modelViewProjectionMatrix, &plane)
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct(&plane, &vertexHomogeneous)

    let rightHalfSpaceCheck
        (modelViewProjectionMatrix : byref<Matrix4x4>)
        (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        Geometry.ExtractRightPlane(&modelViewProjectionMatrix, &plane)
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct(&plane, &vertexHomogeneous)

    let topHalfSpaceCheck
        (modelViewProjectionMatrix : byref<Matrix4x4>) 
        (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        Geometry.ExtractTopPlane(&modelViewProjectionMatrix, &plane)
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct(&plane, &vertexHomogeneous)

    let bottomHalfSpaceCheck
        (modelViewProjectionMatrix : byref<Matrix4x4>)
        (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        Geometry.ExtractBottomPlane(&modelViewProjectionMatrix, &plane)
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct(&plane, &vertexHomogeneous)

    let nearHalfSpaceCheck
        (modelViewProjectionMatrix : byref<Matrix4x4>)
        (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        Geometry.ExtractNearPlane(&modelViewProjectionMatrix, &plane)
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct(&plane, &vertexHomogeneous)

    let farHalfSpaceCheck
        (modelViewProjectionMatrix : byref<Matrix4x4>)
        (plane : byref<Vector4>) 
        (vertexHomogeneous : byref<Vector4>) =
        Geometry.ExtractFarPlane(&modelViewProjectionMatrix, &plane)
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct(&plane, &vertexHomogeneous)

    let IsVertexWithinFrustum(modelViewProjectionMatrix : byref<Matrix4x4>) (vertex : byref<Vector4>) =
        // This vector will be filled for each check. This is not parallizable!
        let mutable plane = Vector4.Zero

        leftHalfSpaceCheck &modelViewProjectionMatrix &plane &vertex &&
        rightHalfSpaceCheck &modelViewProjectionMatrix &plane &vertex &&
        topHalfSpaceCheck &modelViewProjectionMatrix &plane &vertex &&
        bottomHalfSpaceCheck &modelViewProjectionMatrix &plane &vertex &&
        nearHalfSpaceCheck &modelViewProjectionMatrix &plane &vertex &&
        farHalfSpaceCheck &modelViewProjectionMatrix &plane &vertex

    /// <summary>
    /// Culls a <see cref="Henzai.Core.VertexGeometry.Mesh"/> by testing every triangle of the mesh
    /// </summary>
    let FrustumCullMesh (modelViewProjectionMatrix : byref<Matrix4x4>) (mesh : Henzai.Core.VertexGeometry.Mesh<'T>) =
        let processedIndicesMap = new Dictionary<uint16,bool>()

        let vertices = mesh.Vertices
        let indices = mesh.Indices
        let validVertices = mesh.ValidVertices
        let validIndices = mesh.ValidIndices

        let mutable validIndicesCounter = 0
        let mutable validVertexCounter = 0

        for i in 0 .. 3 .. indices.Length-1 do
            let i1 = indices.[i]
            let i2 = indices.[i+1]
            let i3 = indices.[i+2]

            let mutable v1 = vertices.[int i1].GetPosition()
            let mutable v2 = vertices.[int i2].GetPosition()
            let mutable v3 = vertices.[int i3].GetPosition()

            // If at least one vertex is within the frustum, the triangle is not culled.
            if(IsVertexWithinFrustum &modelViewProjectionMatrix &v1 ||
               IsVertexWithinFrustum &modelViewProjectionMatrix &v2 ||
               IsVertexWithinFrustum &modelViewProjectionMatrix &v3) then

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

        mesh.NumberOfValidIndicies <- validIndicesCounter
        mesh.NumberOfValidVertices <- validVertexCounter
        ()