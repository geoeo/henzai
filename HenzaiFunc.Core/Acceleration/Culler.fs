namespace HenzaiFunc.Core.Acceleration

open System.Numerics
open System.Collections.Generic
open Henzai.Core.Numerics
open Henzai.Core.VertexGeometry
open HenzaiFunc.Core.Types
open Henzai.Core.Acceleration

module Culler =
    let halfSpaceCheck (plane : byref<Vector4>)
        (vertexHomogeneous : byref<Vector4>) =
        0.0f < Henzai.Core.Numerics.Vector.InMemoryDotProduct(&plane, &vertexHomogeneous)

    let isLineIntersectionPlane (v : byref<Vector4>) (d : byref<Vector4>) (plane : byref<Vector4>) =
        let a = plane.X
        let b = plane.Y
        let c = plane.Z
        let denom = a*d.X + b*d.Y + c*d.Z
        let num = - (a*v.X + b*v.Y + c*v.Z + plane.W)
        match (denom,num) with 
        | (d,_) when d = 0.0f -> false
        | (d,n) when n/d <= 0.0f -> true
        | _ -> true

    // Works but too expensive
    let AreVerticesIntersectingFrustum (v1 : byref<Vector4>) (v2 : byref<Vector4>) (v3 : byref<Vector4>)
        (planeLeft : byref<Vector4>) (planeRight : byref<Vector4>)
        (planeTop : byref<Vector4>) (planeBottom : byref<Vector4>)
        (planeNear : byref<Vector4>) (planeFar : byref<Vector4>) =
        let mutable l1 = v2 - v1
        let mutable l2 = v3 - v2
        let mutable l3 = v1 - v3
        isLineIntersectionPlane &v1 &l1 &planeLeft ||
        isLineIntersectionPlane &v1 &l1 &planeRight ||
        isLineIntersectionPlane &v1 &l1 &planeTop ||
        isLineIntersectionPlane &v1 &l1 &planeBottom ||
        isLineIntersectionPlane &v1 &l1 &planeNear ||
        isLineIntersectionPlane &v1 &l1 &planeFar ||
        isLineIntersectionPlane &v2 &l2 &planeLeft ||
        isLineIntersectionPlane &v2 &l2 &planeRight ||
        isLineIntersectionPlane &v2 &l2 &planeTop ||
        isLineIntersectionPlane &v2 &l2 &planeBottom ||
        isLineIntersectionPlane &v2 &l2 &planeNear ||
        isLineIntersectionPlane &v2 &l2 &planeFar ||
        isLineIntersectionPlane &v3 &l3 &planeLeft ||
        isLineIntersectionPlane &v3 &l3 &planeRight ||
        isLineIntersectionPlane &v3 &l3 &planeTop ||
        isLineIntersectionPlane &v3 &l3 &planeBottom ||
        isLineIntersectionPlane &v3 &l3 &planeNear ||
        isLineIntersectionPlane &v3 &l3 &planeFar


    let IsVertexWithinFrustum (vertex : byref<Vector4>)
        (planeLeft : byref<Vector4>) (planeRight : byref<Vector4>)
        (planeTop : byref<Vector4>) (planeBottom : byref<Vector4>)
        (planeNear : byref<Vector4>) (planeFar : byref<Vector4>) =
        halfSpaceCheck &planeLeft &vertex
        && halfSpaceCheck &planeRight &vertex
        && halfSpaceCheck &planeTop &vertex
        && halfSpaceCheck &planeBottom &vertex
        && halfSpaceCheck &planeNear &vertex
        && halfSpaceCheck &planeFar &vertex

    let IsVertexWithinNearFarFrustum (vertex : byref<Vector4>)
        (planeNear : byref<Vector4>) (planeFar : byref<Vector4>) =
        halfSpaceCheck &planeNear &vertex
        && halfSpaceCheck &planeFar &vertex

    /// <summary>
    /// Culls a <see cref="Henzai.Core.VertexGeometry.Mesh"/> by testing every triangle of the mesh
    /// This sets internal flags in mesh.
    /// </summary>
    let FrustumCullMesh (worldViewProjectionMatrix : byref<Matrix4x4>)
        (mesh : Henzai.Core.VertexGeometry.Mesh<'T>) =
        //TODO: Profile this thoroughly
        let processedIndicesMap = new Dictionary<uint16,uint16>()
        let vertices = mesh.Vertices
        let indices = mesh.Indices
        let validVertices = mesh.ValidVertices
        let validIndices = mesh.ValidIndices
        let mutable validIndicesCounter = 0us
        let mutable validVertexCounter = 0
        // let mutable planeLeft =
        //     Geometry.ExtractLeftPlane(&worldViewProjectionMatrix)
        // let mutable planeRight =
        //     Geometry.ExtractRightPlane(&worldViewProjectionMatrix)
        // let mutable planeTop =
        //     Geometry.ExtractTopPlane(&worldViewProjectionMatrix)
        // let mutable planeBottom =
        //     Geometry.ExtractBottomPlane(&worldViewProjectionMatrix)
        let mutable planeNear =
            Geometry.ExtractNearPlane(&worldViewProjectionMatrix)
        let mutable planeFar =
            Geometry.ExtractFarPlane(&worldViewProjectionMatrix)
        for i in 0..3..indices.Length - 1 do
            let i1 = indices.[i]
            let i2 = indices.[i + 1]
            let i3 = indices.[i + 2]
            let vertex1 = vertices.[int i1]
            let vertex2 = vertices.[int i2]
            let vertex3 = vertices.[int i3]
            let mutable v1 = vertex1.GetPosition()
            let mutable v2 = vertex2.GetPosition()
            let mutable v3 = vertex3.GetPosition()
            // If at least one vertex is within the frustum, the triangle is not culled.
            if (IsVertexWithinNearFarFrustum &v1 &planeNear &planeFar
                || IsVertexWithinNearFarFrustum &v2 &planeNear &planeFar
                || IsVertexWithinNearFarFrustum &v3 &planeNear &planeFar) then
                if (not (processedIndicesMap.ContainsKey(i1))) then
                    processedIndicesMap.Add(i1, (uint16)validVertexCounter)
                    validVertices.[validVertexCounter] <- vertex1
                    validIndices.[(int)validIndicesCounter] <- (uint16)validVertexCounter
                    validVertexCounter <- validVertexCounter + 1
                else
                    validIndices.[(int)validIndicesCounter] <- processedIndicesMap.Item(i1)
                validIndicesCounter <- validIndicesCounter + 1us
                if (not (processedIndicesMap.ContainsKey(i2))) then
                    processedIndicesMap.Add(i2, (uint16)validVertexCounter)
                    validVertices.[validVertexCounter] <- vertex2
                    validIndices.[(int)validIndicesCounter] <- (uint16)validVertexCounter
                    validVertexCounter <- validVertexCounter + 1
                else
                    validIndices.[(int)validIndicesCounter] <- processedIndicesMap.Item(i2)
                validIndicesCounter <- validIndicesCounter + 1us
                if (not (processedIndicesMap.ContainsKey(i3))) then
                    processedIndicesMap.Add(i3, (uint16)validVertexCounter)
                    validVertices.[validVertexCounter] <- vertex3
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
                mesh.AABBIsValid <- true;
            ()
        else
            let leftChildIndex = runtimeNodeIndex + 1
            let rightChildIndex = runtimeNode.interiorNode.secondChildOffset
            processInteriorNodeForCulling leftChildIndex bvhArray orderedPrimitives
            processInteriorNodeForCulling rightChildIndex bvhArray orderedPrimitives
            ()

    /// <summary>
    /// Culls a list of <see cref="Henzai.Core.Acceleration.IndexedTriangleEngine"/> with the camera view frustum
    /// </summary>
    let FrustumCullBVH (bvhArray : BVHRuntimeNode[], orderedPrimitives : IndexedTriangleEngine<'T>[], worldViewProjectionMatrix : byref<Matrix4x4>) =
        let bvhTraversalStack = Array.zeroCreate orderedPrimitives.Length

        let stopWatch = System.Diagnostics.Stopwatch.StartNew()
        let indices = BVHRuntime.TraverseWithFrustum(bvhArray, bvhTraversalStack, &worldViewProjectionMatrix)
        stopWatch.Stop()
        let perfTimeBVHTraverse = stopWatch.Elapsed.TotalMilliseconds

        stopWatch.Restart();
        match indices with
        | struct(-1, -1) -> () // everything is culled
        | struct(indexA, -1) -> processInteriorNodeForCulling indexA bvhArray orderedPrimitives
        | struct(indexA, indexB) ->
            processInteriorNodeForCulling indexA bvhArray orderedPrimitives
            processInteriorNodeForCulling indexB bvhArray orderedPrimitives
        stopWatch.Stop();
        let perfTimeNodeProcessing = stopWatch.Elapsed.TotalMilliseconds
        ()



