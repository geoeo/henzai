namespace HenzaiFunc.Core.Acceleration

open System.Numerics
open System.Collections.Generic
open Henzai.Core.Numerics
open Henzai.Core.VertexGeometry
open HenzaiFunc.Core.Types
open Henzai.Core.Acceleration
open System

module Culler =
    let leftHalfSpaceCheck (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct
                   (&plane, &vertexHomogeneous)
    let rightHalfSpaceCheck (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct
                   (&plane, &vertexHomogeneous)
    let topHalfSpaceCheck (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct
                   (&plane, &vertexHomogeneous)
    let bottomHalfSpaceCheck (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct
                   (&plane, &vertexHomogeneous)
    let nearHalfSpaceCheck (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct
                   (&plane, &vertexHomogeneous)
    let farHalfSpaceCheck (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct
                   (&plane, &vertexHomogeneous)
    let IsVertexWithinFrustum (vertex : byref<Vector4>)
        (planeLeft : byref<Vector4>) (planeRight : byref<Vector4>)
        (planeTop : byref<Vector4>) (planeBottom : byref<Vector4>)
        (planeNear : byref<Vector4>) (planeFar : byref<Vector4>) =
        leftHalfSpaceCheck &planeLeft &vertex
        && rightHalfSpaceCheck &planeRight &vertex
        && topHalfSpaceCheck &planeTop &vertex
        && bottomHalfSpaceCheck &planeBottom &vertex
        && nearHalfSpaceCheck &planeNear &vertex
        && farHalfSpaceCheck &planeFar &vertex

    /// <summary>
    /// Culls a <see cref="Henzai.Core.VertexGeometry.Mesh"/> by testing every triangle of the mesh
    /// </summary>
    let FrustumCullMesh (viewProjectionMatrix : byref<Matrix4x4>)
        (mesh : Henzai.Core.VertexGeometry.Mesh<'T>) =
        //TODO: Profile this thoroughly
        let processedIndicesMap = new Dictionary<uint16,uint16>()
        let vertices = mesh.Vertices
        let indices = mesh.Indices
        let validVertices = mesh.ValidVertices
        let validIndices = mesh.ValidIndices
        let mutable validIndicesCounter = 0us
        let mutable validVertexCounter = 0
        let mutable planeLeft =
            Geometry.ExtractLeftPlane(&viewProjectionMatrix)
        let mutable planeRight =
            Geometry.ExtractRightPlane(&viewProjectionMatrix)
        let mutable planeTop =
            Geometry.ExtractTopPlane(&viewProjectionMatrix)
        let mutable planeBottom =
            Geometry.ExtractBottomPlane(&viewProjectionMatrix)
        let mutable planeNear =
            Geometry.ExtractNearPlane(&viewProjectionMatrix)
        let mutable planeFar =
            Geometry.ExtractFarPlane(&viewProjectionMatrix)
        for i in 0..3..indices.Length - 1 do
            let i1 = indices.[i]
            let i2 = indices.[i + 1]
            let i3 = indices.[i + 2]
            let mutable v1 = vertices.[int i1].GetPosition()
            let mutable v2 = vertices.[int i2].GetPosition()
            let mutable v3 = vertices.[int i3].GetPosition()
            // If at least one vertex is within the frustum, the triangle is not culled.
            if (IsVertexWithinFrustum &v1 &planeLeft &planeRight &planeTop
                    &planeBottom &planeNear &planeFar
                || IsVertexWithinFrustum &v2 &planeLeft &planeRight &planeTop
                       &planeBottom &planeNear &planeFar
                || IsVertexWithinFrustum &v3 &planeLeft &planeRight &planeTop
                       &planeBottom &planeNear &planeFar) then
                if (not (processedIndicesMap.ContainsKey(i1))) then
                    processedIndicesMap.Add(i1, (uint16)validVertexCounter)
                    validVertices.[validVertexCounter] <- vertices.[int i1]
                    validIndices.[(int)validIndicesCounter] <- (uint16)validVertexCounter
                    validVertexCounter <- validVertexCounter + 1
                else
                    validIndices.[(int)validIndicesCounter] <- processedIndicesMap.Item(i1)
                validIndicesCounter <- validIndicesCounter + 1us
                if (not (processedIndicesMap.ContainsKey(i2))) then
                    processedIndicesMap.Add(i2, (uint16)validVertexCounter)
                    validVertices.[validVertexCounter] <- vertices.[int i2]
                    validIndices.[(int)validIndicesCounter] <- (uint16)validVertexCounter
                    validVertexCounter <- validVertexCounter + 1
                else
                    validIndices.[(int)validIndicesCounter] <- processedIndicesMap.Item(i2)
                validIndicesCounter <- validIndicesCounter + 1us
                if (not (processedIndicesMap.ContainsKey(i3))) then
                    processedIndicesMap.Add(i3, (uint16)validVertexCounter)
                    validVertices.[validVertexCounter] <- vertices.[int i3]
                    validIndices.[(int)validIndicesCounter] <- (uint16)validVertexCounter
                    validVertexCounter <- validVertexCounter + 1
                else
                    validIndices.[(int)validIndicesCounter] <- processedIndicesMap.Item(i3)
                validIndicesCounter <- validIndicesCounter + 1us
        mesh.ValidIndexCount <- (int)validIndicesCounter
        mesh.ValidVertexCount <- validVertexCounter
        ()

    let rec processInteriorNodeForCulling (runtimeNodeIndex : int) (bvhArray : BVHRuntimeNode[]) (orderedPrimitives : IndexedTriangleEngine<'T>[]) =
        let runtimeNode = bvhArray.[runtimeNodeIndex]
        let nPrimitives = runtimeNode.nPrimitives
        if nPrimitives > 0 then
            let primitiveOffset = runtimeNode.leafNode.primitivesOffset
            for j in 0..nPrimitives-1 do
            let mesh = orderedPrimitives.[primitiveOffset+j].Mesh
            let processedIndicesMap = new Dictionary<uint16,uint16>()
            let vertices = mesh.Vertices
            let indices = mesh.Indices
            let validVertices = mesh.ValidVertices
            let validIndices = mesh.ValidIndices
            let mutable validIndicesCounter = 0us
            let mutable validVertexCounter = 0
            // Every vertex in Intersecting or Inside
            for i in 0..3..indices.Length-1 do
                let i1 = indices.[i]
                let i2 = indices.[i + 1]
                let i3 = indices.[i + 2]
                let mutable v1 = vertices.[int i1].GetPosition()
                let mutable v2 = vertices.[int i2].GetPosition()
                let mutable v3 = vertices.[int i3].GetPosition()
                if (not (processedIndicesMap.ContainsKey(i1))) then
                    processedIndicesMap.Add(i1, (uint16)validVertexCounter)
                    validVertices.[validVertexCounter] <- vertices.[int i1]
                    validIndices.[(int)validIndicesCounter] <- (uint16)validVertexCounter
                    validVertexCounter <- validVertexCounter + 1
                else
                    validIndices.[(int)validIndicesCounter] <- processedIndicesMap.Item(i1)
                validIndicesCounter <- validIndicesCounter + 1us
                if (not (processedIndicesMap.ContainsKey(i2))) then
                    processedIndicesMap.Add(i2, (uint16)validVertexCounter)
                    validVertices.[validVertexCounter] <- vertices.[int i2]
                    validIndices.[(int)validIndicesCounter] <- (uint16)validVertexCounter
                    validVertexCounter <- validVertexCounter + 1
                else
                    validIndices.[(int)validIndicesCounter] <- processedIndicesMap.Item(i2)
                validIndicesCounter <- validIndicesCounter + 1us
                if (not (processedIndicesMap.ContainsKey(i3))) then
                    processedIndicesMap.Add(i3, (uint16)validVertexCounter)
                    validVertices.[validVertexCounter] <- vertices.[int i3]
                    validIndices.[(int)validIndicesCounter] <- (uint16)validVertexCounter
                    validVertexCounter <- validVertexCounter + 1
                else
                    validIndices.[(int)validIndicesCounter] <- processedIndicesMap.Item(i3)
                validIndicesCounter <- validIndicesCounter + 1us
            mesh.ValidIndexCount <- (int)validIndicesCounter
            mesh.ValidVertexCount <- validVertexCounter
            ()
        else
            let leftChildIndex = runtimeNodeIndex+1
            let rightChildIndex = runtimeNode.interiorNode.secondChildOffset
            processInteriorNodeForCulling leftChildIndex bvhArray orderedPrimitives
            processInteriorNodeForCulling rightChildIndex bvhArray orderedPrimitives
            ()

    /// <summary>
    /// Culls a list of <see cref="Henzai.Core.Acceleration.IndexedTriangleEngine"/> with the camera view frustum
    /// </summary>
    let FrustumCullBVH (bvhArray : BVHRuntimeNode[], orderedPrimitives : IndexedTriangleEngine<'T>[],nodesToVisit : int[], worldViewProjectionMatrix : byref<Matrix4x4>) =
        let indices = BVHRuntime.TraverseWithFrustum(bvhArray, orderedPrimitives,nodesToVisit, &worldViewProjectionMatrix)
        match indices with
        | struct(-1, -1) -> ()
        | struct(indexA, -1) -> processInteriorNodeForCulling indexA bvhArray orderedPrimitives
        | struct(indexA, indexB) ->
            processInteriorNodeForCulling indexA bvhArray orderedPrimitives
            processInteriorNodeForCulling indexB bvhArray orderedPrimitives


