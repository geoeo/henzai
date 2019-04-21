namespace HenzaiFunc.Core.Acceleration

open System.Numerics
open HenzaiFunc.Core.Types
open Henzai.Core.Acceleration
open Henzai.Core.Raytracing

// Phyisically Based Rendering Third Edition p. 280
/// Implements methods to optimize a BVH BST into a runtime representation
type BVHRuntime<'T when 'T :> Hitable>() = 
   
    let determineSubTreeOrder ((l, r) : (BVHTree * BVHTree)) (splitAxis : SplitAxis) (coordinateSystem : CoordinateSystem) =
        match splitAxis, coordinateSystem with
        | SplitAxis.X, CoordinateSystem.XYNegZ -> (l, r)
        | SplitAxis.Y, CoordinateSystem.XYNegZ -> (l, r)
        | SplitAxis.Z, CoordinateSystem.XYNegZ -> (r, l)
        | SplitAxis.X, CoordinateSystem.XYZ -> (l, r)
        | SplitAxis.Y, CoordinateSystem.XYZ -> (l, r)
        | SplitAxis.Z, CoordinateSystem.XYZ -> (l, r)
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
    let rec flattenBVHTreeRec (bvhTree : BVHTree) (bvhRuntimeArray : BVHRuntimeNode []) (currentOffset : int) (coordinateSystem : CoordinateSystem) =
        if bvhRuntimeArray.Length = 0 then
            (0,0)
        else
            let (currentNode, leftSubTree, rightSubTree) = BVHTree.decompose bvhTree
            let currentBounds = currentNode.aabb
            let firstChild = currentOffset + 1
            if currentNode.nPrimitives > 0 then
                let leafRuntimeNode = LeafRuntimeNode(currentNode.firstPrimitiveOffset)
                bvhRuntimeArray.[currentOffset] <- BVHRuntimeNode(currentBounds, leafRuntimeNode, currentNode.nPrimitives)
                (currentOffset, 1)
            else
                let splitAxis = currentNode.splitAxis
                let (closerSubTree, fartherSubTree) = determineSubTreeOrder (leftSubTree, rightSubTree) splitAxis coordinateSystem
                let (firstChildOffset, count) = 
                    flattenBVHTreeRec closerSubTree bvhRuntimeArray firstChild coordinateSystem

                let (secondChildOffset, count2) = 
                    flattenBVHTreeRec fartherSubTree bvhRuntimeArray (firstChildOffset + count) coordinateSystem

                let interiorRuntimeNode = InteriorRuntimeNode(secondChildOffset, splitAxis)
                bvhRuntimeArray.[currentOffset] <- BVHRuntimeNode(currentBounds, interiorRuntimeNode, 0)
                //TODO: this is buggy
                (currentOffset, count + count2 + 1)

    member this.allocateMemoryForBVHRuntime nodeCount = 
        Array.zeroCreate<BVHRuntimeNode> nodeCount

    /// Allocates memory and flattens the array
    member this.constructBVHRuntime bvhTree nodeCount =
        let bvhArray = this.allocateMemoryForBVHRuntime nodeCount
        flattenBVHTreeRec bvhTree bvhArray 0 CoordinateSystem.XYNegZ |> ignore
        bvhArray

    member this.flattenBVHTree (bvhTree : BVHTree) (bvhRuntimeArray : BVHRuntimeNode []) (currentOffset : int) (coordinateSystem : CoordinateSystem) = 
        flattenBVHTreeRec bvhTree bvhRuntimeArray currentOffset coordinateSystem

    /// Finds the closest intersection for the given ray
    member this.traverse (bvhArray : BVHRuntimeNode []) (orderedPrimitives : 'T []) (nodesToVisit : int[]) (ray : Ray) = 
        let invDir = Vector4(1.0f / ray.Direction.X, 1.0f/ ray.Direction.Y, 1.0f / ray.Direction.Z, 0.0f)
        let (isXDirNeg, isYDirNeg, isZDirNeg) = (invDir.X < 0.0f, invDir.Y < 0.0f, invDir.Z < 0.0f)
        let mutable toVisitOffset = 0
        let mutable currentNodeIndex = 0
        let mutable hasIntersection = false
        let mutable tHit = System.Single.MaxValue
        let mutable intersectedGeometry : ('T option) = None
       
        while toVisitOffset >= 0 do
            let node = bvhArray.[currentNodeIndex]
            let primitiveOffset = node.leafNode.primitivesOffset
            let nPrimitives = node.nPrimitives
            if node.aabb.HasIntersection(ray) then 
                // leaf
                if nPrimitives > 0 then
                    for i in 0..nPrimitives-1 do
                        let geometry = orderedPrimitives.[primitiveOffset + i]
                        let struct(leafHasIntersection, leafHit) = geometry.Intersect(ray) 
                        let point = ray.Origin + leafHit*ray.Direction
                        let intersectionAcceptable = geometry.IntersectionAcceptable(leafHasIntersection, leafHit, 1.0f, point)
                        if intersectionAcceptable && leafHit < tHit then
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
        struct(hasIntersection, tHit, intersectedGeometry)


        
            
        
        
        
        
