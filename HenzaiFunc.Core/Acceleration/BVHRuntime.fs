namespace HenzaiFunc.Core.Acceleration

open System.Numerics
open HenzaiFunc.Core.Types
open HenzaiFunc.Core.RaytraceGeometry

// Phyisically Based Rendering Third Edition p. 280
/// Implements methods to optimize a BVH BST into a runtime representation
module BVHRuntime = 

    let allocateMemoryForBVHRuntime nodeCount = 
        Array.zeroCreate<BVHRuntimeNode> nodeCount

    let determineSubTreeOrder ((l, r) : (BVHTree * BVHTree)) (splitAxis : SplitAxis) (coordinateSystem : CoordinateSystem) =
        match splitAxis, coordinateSystem with
        | SplitAxis.X, CoordinateSystem.XYNegZ -> (l, r)
        | SplitAxis.Y, CoordinateSystem.XYNegZ -> (l, r)
        | SplitAxis.Z, CoordinateSystem.XYNegZ -> (r, l)
        | (_, x) -> failwithf "coordinate system %u for subtree not implemented!" (LanguagePrimitives.EnumToValue x)

    let accessBySplitAxis (v1, v2, v3) axis = 
        match axis with
        | SplitAxis.X -> v1
        | SplitAxis.Y -> v2
        | SplitAxis.Z -> v3
        | x -> failwithf "Accessed point by %u. Should not happen!" (LanguagePrimitives.EnumToValue x)

    /// Flattens the BVHTree into the supplied bvhRuntimeArray.
    /// Returns 1. This is an artefact of the C++ implementation it is based on.
    //TODO: Refactor to make more functional
    let rec flattenBVHTree (bvhTree : BVHTree) (bvhRuntimeArray : BVHRuntimeNode []) (currentOffset : int) (coordinateSystem : CoordinateSystem) = 
        let (currentNode, leftSubTree, rightSubTree) = BVHTree.decompose bvhTree
        let currentBounds = currentNode.aabb
        if currentNode.nPrimitives > 0 then
            let leafRuntimeNode = LeafRuntimeNode(currentNode.firstPrimitiveOffset)
            bvhRuntimeArray.[currentOffset] <- BVHRuntimeNode(currentBounds, leafRuntimeNode, currentNode.nPrimitives)
            currentOffset
        else
            let splitAxis = currentNode.splitAxis
            let (closerSubTree, fartherSubTree) = determineSubTreeOrder (leftSubTree, rightSubTree) splitAxis coordinateSystem
            let firstChildOffset = flattenBVHTree closerSubTree bvhRuntimeArray (currentOffset + 1) coordinateSystem
            let secondChildOffset = flattenBVHTree fartherSubTree bvhRuntimeArray (firstChildOffset + 1) coordinateSystem
            let interiorRuntimeNode = InteriorRuntimeNode(secondChildOffset, splitAxis)
            bvhRuntimeArray.[currentOffset] <- BVHRuntimeNode(currentBounds, interiorRuntimeNode, 0)
            secondChildOffset

    let traverse (bvhArray : BVHRuntimeNode []) (orderedPrimitives : RaytracingGeometry []) (ray : Ray ) = 
        let invDir = Vector3(1.0f / ray.Direction.X, 1.0f/ ray.Direction.Y, 1.0f / ray.Direction.Z)
        let struct(isXDirNeg, isYDirNeg, isZDirNeg) = struct(invDir.X < 0.0f, invDir.Y < 0.0f, invDir.Z < 0.0f)
        let mutable toVisitOffset = 0
        let mutable currentNodeIndex = 0
        let mutable hasIntersection = false
        let mutable tHit = 0.0f
        let mutable intersectedGeometry : (RaytracingGeometry option) = None
        let mutable nodesToVisit = Array.zeroCreate bvhArray.Length

        while toVisitOffset >= 0 do
            let node = bvhArray.[currentNodeIndex]
            let nPrimitives = node.nPrimitives
            if node.aabb.AsHitable.HasIntersection ray then
                // leaf
                if nPrimitives > 0 then
                    for i in 0..nPrimitives-1 do
                        let geometry = orderedPrimitives.[node.leafNode.primitivesOffset + i]
                        let (leafHasIntersection, leafHit) = geometry.AsHitable.Intersect ray
                        hasIntersection <- leafHasIntersection
                        tHit <- leafHit
                        intersectedGeometry <- Some geometry
                    toVisitOffset <- toVisitOffset - 1
                    if toVisitOffset >= 0 then 
                        currentNodeIndex <- nodesToVisit.[toVisitOffset]
                // interor node
                else
                    let interiorNode = node.interiorNode
                    if accessBySplitAxis (isXDirNeg, isYDirNeg, isZDirNeg) interiorNode.splitAxis then
                        nodesToVisit.[toVisitOffset] <- currentNodeIndex + 1
                        toVisitOffset <- toVisitOffset + 1 
                        currentNodeIndex <- interiorNode.secondChildOffset
                    else
                        nodesToVisit.[toVisitOffset] <- interiorNode.secondChildOffset
                        toVisitOffset <- toVisitOffset + 1 
                        currentNodeIndex <- currentNodeIndex + 1 

             else
                 toVisitOffset <- toVisitOffset - 1
                 if toVisitOffset >= 0 then 
                     currentNodeIndex <- nodesToVisit.[toVisitOffset]
           
            

        (hasIntersection, tHit, intersectedGeometry)


        
            
        
        
        
        
