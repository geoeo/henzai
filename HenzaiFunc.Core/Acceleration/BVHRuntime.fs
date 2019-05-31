namespace HenzaiFunc.Core.Acceleration

open System.Numerics
open HenzaiFunc.Core.Types
open Henzai.Core.Acceleration
open Henzai.Core.Raytracing
open Henzai.Core.Numerics

// Phyisically Based Rendering Third Edition p. 280
/// Implements methods to optimize a BVH BST into a runtime representation
[<AbstractClass; Sealed>]
type BVHRuntime private() = 
   
    static let determineSubTreeOrder ((l, r) : (BVHTree * BVHTree)) (splitAxis : SplitAxis) (coordinateSystem : CoordinateSystem) =
        match splitAxis, coordinateSystem with
        | SplitAxis.X, CoordinateSystem.XYNegZ -> (l, r)
        | SplitAxis.Y, CoordinateSystem.XYNegZ -> (l, r)
        | SplitAxis.Z, CoordinateSystem.XYNegZ -> (r, l)
        | SplitAxis.X, CoordinateSystem.XYZ -> (l, r)
        | SplitAxis.Y, CoordinateSystem.XYZ -> (l, r)
        | SplitAxis.Z, CoordinateSystem.XYZ -> (l, r)
        | (_, x) -> failwithf "coordinate system %u for subtree not implemented!" (LanguagePrimitives.EnumToValue x)

    static let accessBySplitAxis (v1, v2, v3) axis = 
        match axis with
        | SplitAxis.X -> v1
        | SplitAxis.Y -> v2
        | SplitAxis.Z -> v3
        | x -> failwithf "Accessed point by %u. Should not happen!" (LanguagePrimitives.EnumToValue x)

    /// Flattens the BVHTree into the supplied bvhRuntimeArray.
    static let rec flattenBVHTreeRec (bvhTree : BVHTree) (bvhRuntimeArray : BVHRuntimeNode []) (currentOffset : int) (coordinateSystem : CoordinateSystem) =
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
                (currentOffset, count + count2 + 1)

    static member AllocateMemoryForBVHRuntime nodeCount = 
        Array.zeroCreate<BVHRuntimeNode> nodeCount

    /// Allocates memory and flattens the array
    static member ConstructBVHRuntime bvhTree nodeCount =
        let bvhArray = BVHRuntime.AllocateMemoryForBVHRuntime nodeCount
        flattenBVHTreeRec bvhTree bvhArray 0 CoordinateSystem.XYNegZ |> ignore
        bvhArray

    static member FlattenBVHTree (bvhTree : BVHTree) (bvhRuntimeArray : BVHRuntimeNode []) (currentOffset : int) (coordinateSystem : CoordinateSystem) = 
        flattenBVHTreeRec bvhTree bvhRuntimeArray currentOffset coordinateSystem

    /// Finds the closest intersection for the given ray
    static member Traverse<'T when 'T :> Hitable> (bvhArray : BVHRuntimeNode []) (orderedPrimitives : 'T []) (nodesToVisit : int[]) (ray : Ray) = 
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


    /// Finds the closest intersection for the given ray
    static member TraverseForZCulling (bvhArray : BVHRuntimeNode []) (orderedPrimitives : IndexedTriangleBVH<'T> []) (nodesToVisit : int[]) (ray : Ray) = 
        let mutable toVisitOffset = 0
        let mutable currentNodeIndex = 0

        while toVisitOffset >= 0 do
            let node = bvhArray.[currentNodeIndex]
            let nPrimitives = node.nPrimitives
            if node.aabb.HasIntersection(ray) then 
                // leaf
                if nPrimitives > 0 then
                    let primitiveOffset = node.leafNode.primitivesOffset
                    for j in 0..nPrimitives-1 do
                        let mutable indexedTriangle = orderedPrimitives.[primitiveOffset+j]
                        indexedTriangle.AABBIsValid <- true
                        orderedPrimitives.[primitiveOffset+j] <- indexedTriangle
                    toVisitOffset <- toVisitOffset - 1
                    if toVisitOffset >= 0 then 
                        currentNodeIndex <- nodesToVisit.[toVisitOffset]
                // interor node
                else
                    let interiorNode = node.interiorNode
                    nodesToVisit.[toVisitOffset] <- currentNodeIndex + 1
                    toVisitOffset <- toVisitOffset + 1 
                    currentNodeIndex <- interiorNode.secondChildOffset
            else
                toVisitOffset <- toVisitOffset - 1
                if toVisitOffset >= 0 then 
                    currentNodeIndex <- nodesToVisit.[toVisitOffset]
        ()

    static member TraverseWithFrustum (bvhArray : BVHRuntimeNode[], orderedPrimitives : IndexedTriangleBVH<'T> [], nodesToVisit : int[], viewProjectionMatrix : byref<Matrix4x4>) =
        let mutable toVisitOffset = 0
        let mutable currentNodeIndex = 0

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

        while toVisitOffset >= 0 do
            let node = bvhArray.[currentNodeIndex]
            let nPrimitives = node.nPrimitives
            let currentAABB = node.aabb

            // These impact performance a little
            let currentIntersectionLeft = AABBProc.PlaneIntersection(currentAABB,planeLeft)
            let currentIntersectionRight = AABBProc.PlaneIntersection(currentAABB,planeRight)
            let currentIntersectionTop = AABBProc.PlaneIntersection(currentAABB,planeTop)
            let currentIntersectionBottom = AABBProc.PlaneIntersection(currentAABB,planeBottom)
            let currentIntersectionNear = AABBProc.PlaneIntersection(currentAABB,planeNear)
            let currentIntersectionFar = AABBProc.PlaneIntersection(currentAABB,planeFar)

            let inside = 
                currentIntersectionNear = IntersectionResult.Inside &&
                currentIntersectionFar = IntersectionResult.Inside &&
                currentIntersectionLeft = IntersectionResult.Inside &&
                currentIntersectionRight = IntersectionResult.Inside && 
                currentIntersectionTop = IntersectionResult.Inside &&
                currentIntersectionBottom = IntersectionResult.Inside

            let intersecting = 
                currentIntersectionNear = IntersectionResult.Intersecting ||
                currentIntersectionFar = IntersectionResult.Intersecting ||
                currentIntersectionLeft = IntersectionResult.Intersecting ||
                currentIntersectionRight = IntersectionResult.Intersecting ||
                currentIntersectionTop = IntersectionResult.Intersecting ||
                currentIntersectionBottom = IntersectionResult.Intersecting

            // "or"-ing seems to faster than "not"-ing i.e. not outside
            let outside = 
                currentIntersectionNear = IntersectionResult.Outside &&
                currentIntersectionFar = IntersectionResult.Outside &&
                currentIntersectionLeft = IntersectionResult.Outside &&
                currentIntersectionRight = IntersectionResult.Outside && 
                currentIntersectionTop = IntersectionResult.Outside &&
                currentIntersectionBottom = IntersectionResult.Outside

            if (inside || intersecting) then 
                if nPrimitives > 0 then 
                    let primitiveOffset = node.leafNode.primitivesOffset
                    for j in 0..nPrimitives-1 do
                        let mutable meshBVH = orderedPrimitives.[primitiveOffset+j]
                        meshBVH.AABBIsValid <- true
                        orderedPrimitives.[primitiveOffset+j] <- meshBVH
                    toVisitOffset <- toVisitOffset - 1
                    if toVisitOffset >= 0 then 
                        currentNodeIndex <- nodesToVisit.[toVisitOffset]
                else
                    let interiorNode = node.interiorNode
                    nodesToVisit.[toVisitOffset] <- currentNodeIndex + 1
                    toVisitOffset <- toVisitOffset + 1 
                    currentNodeIndex <- interiorNode.secondChildOffset
            else 
                toVisitOffset <- toVisitOffset - 1
                if toVisitOffset >= 0 then 
                    currentNodeIndex <- nodesToVisit.[toVisitOffset]
        ()


    static member TraverseWithFrustumForMesh (bvhArray : BVHRuntimeNode[], orderedPrimitives : MeshBVH<'T> [], nodesToVisit : int[], viewProjectionMatrix : byref<Matrix4x4>) =
        let mutable toVisitOffset = 0
        let mutable currentNodeIndex = 0

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

        while toVisitOffset >= 0 do
            let node = bvhArray.[currentNodeIndex]
            let nPrimitives = node.nPrimitives
            let currentAABB = node.aabb

            // These impact performance a little
            let currentIntersectionLeft = AABBProc.PlaneIntersection(currentAABB,planeLeft)
            let currentIntersectionRight = AABBProc.PlaneIntersection(currentAABB,planeRight)
            let currentIntersectionTop = AABBProc.PlaneIntersection(currentAABB,planeTop)
            let currentIntersectionBottom = AABBProc.PlaneIntersection(currentAABB,planeBottom)
            let currentIntersectionNear = AABBProc.PlaneIntersection(currentAABB,planeNear)
            let currentIntersectionFar = AABBProc.PlaneIntersection(currentAABB,planeFar)

            let inside = 
                currentIntersectionNear = IntersectionResult.Inside &&
                currentIntersectionFar = IntersectionResult.Inside &&
                currentIntersectionLeft = IntersectionResult.Inside &&
                currentIntersectionRight = IntersectionResult.Inside && 
                currentIntersectionTop = IntersectionResult.Inside &&
                currentIntersectionBottom = IntersectionResult.Inside

            let intersecting = 
                currentIntersectionNear = IntersectionResult.Intersecting ||
                currentIntersectionFar = IntersectionResult.Intersecting ||
                currentIntersectionLeft = IntersectionResult.Intersecting ||
                currentIntersectionRight = IntersectionResult.Intersecting ||
                currentIntersectionTop = IntersectionResult.Intersecting ||
                currentIntersectionBottom = IntersectionResult.Intersecting

            // "or"-ing seems to faster than "not"-ing i.e. not outside
            let outside = 
                currentIntersectionNear = IntersectionResult.Outside &&
                currentIntersectionFar = IntersectionResult.Outside &&
                currentIntersectionLeft = IntersectionResult.Outside &&
                currentIntersectionRight = IntersectionResult.Outside && 
                currentIntersectionTop = IntersectionResult.Outside &&
                currentIntersectionBottom = IntersectionResult.Outside

            if (inside || intersecting) then 
                if nPrimitives > 0 then 
                    let primitiveOffset = node.leafNode.primitivesOffset
                    for j in 0..nPrimitives-1 do
                        let mutable meshBVH = orderedPrimitives.[primitiveOffset+j]
                        meshBVH.AABBIsValid <- true
                        orderedPrimitives.[primitiveOffset+j] <- meshBVH
                    toVisitOffset <- toVisitOffset - 1
                    if toVisitOffset >= 0 then 
                        currentNodeIndex <- nodesToVisit.[toVisitOffset]
                else
                    let interiorNode = node.interiorNode
                    nodesToVisit.[toVisitOffset] <- currentNodeIndex + 1
                    toVisitOffset <- toVisitOffset + 1 
                    currentNodeIndex <- interiorNode.secondChildOffset
            else 
                toVisitOffset <- toVisitOffset - 1
                if toVisitOffset >= 0 then 
                    currentNodeIndex <- nodesToVisit.[toVisitOffset]
        ()



        
            
        
        
        
        
