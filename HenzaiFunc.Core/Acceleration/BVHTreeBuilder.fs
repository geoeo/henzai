namespace HenzaiFunc.Core.Acceleration

open HenzaiFunc.Core.Types
open HenzaiFunc.Core.Extensions.Array
open Henzai.Core.Acceleration

// Phyisically Based Rendering Third Edition p. 257
/// Implements methods to generate a BVH BST
type BVHTreeBuilder<'T when 'T :> AxisAlignedBoundable>() =

    let minPrimitivesForSplit = 128

    let leafCostForSplit nPrimitives = float32 nPrimitives

    let buildBVHInfoArray ( geometryArray : 'T [] ) =
        Array.mapi (fun i (elem : 'T) -> BVHPrimitive(i, elem.GetBounds())) geometryArray

    let accessPointBySplitAxis (p : Point) axis = 
        match axis with
        | SplitAxis.X -> p.X
        | SplitAxis.Y -> p.Y
        | SplitAxis.Z -> p.Z
        | x -> failwithf "Accessed point by %u. Should not happen!" (LanguagePrimitives.EnumToValue x)

    /// start and finish are offsets indexing the global bvhInfoArray. bvhInfoSubArray is just a slice of that array and not indexed by start and finish
    let rec calculateSplitPoint (bvhInfoSubArray : BVHPrimitive []) (start : int) (finish : int) (centroidBounds : AABB) (dim : SplitAxis) (splitMethod : SplitMethods) =
        match splitMethod with
        | SplitMethods.Middle ->
            let midFloat = (accessPointBySplitAxis centroidBounds.PMin dim + accessPointBySplitAxis centroidBounds.PMax dim) / 2.0f
            //TODO: investigate in place solution without allocating
            let smaller, largerOrEqual = Array.partition (fun (elem : BVHPrimitive) -> accessPointBySplitAxis (AABBProc.Center(elem.aabb)) dim < midFloat) bvhInfoSubArray
            let mid = start + smaller.Length
            struct(mid , smaller, largerOrEqual)
        | SplitMethods.EqualCounts ->
            let mid = bvhInfoSubArray.Length / 2
            let map (e : BVHPrimitive) = accessPointBySplitAxis (AABBProc.Center(e.aabb)) dim
            bvhInfoSubArray.nthElement(map, mid)
            let smaller = bvhInfoSubArray.[..(mid-1)]
            let largerOrEqual = bvhInfoSubArray.[mid..]
            struct(start+mid, smaller, largerOrEqual)
        | SplitMethods.SAH ->
            let nPrimitives = finish - start
            if nPrimitives <= 4 then
                calculateSplitPoint bvhInfoSubArray start finish centroidBounds dim SplitMethods.EqualCounts
            else
                let nBuckets = 12
                let nBuckets_f32 = float32 nBuckets
                let traversalCost = 0.125f
                let bucketCounts : int [] = Array.zeroCreate nBuckets
                let bucketBounds : AABB [] = Array.init nBuckets (fun i -> AABB())
                let bucketCosts : float32 [] = Array.zeroCreate (nBuckets-1)
                let offsetFromIndex aabb = accessPointBySplitAxis (AABBProc.Offset(centroidBounds, AABBProc.Center(aabb))) dim
                // Init
                for i in 0..nPrimitives-1 do
                    let aabbOfIndex = bvhInfoSubArray.[i].aabb
                    let offset = offsetFromIndex aabbOfIndex
                    let b = if offset = 1.0f then nBuckets - 1 else nBuckets*(int offset)
                    bucketCounts.[b] <- bucketCounts.[b] + 1
                    bucketBounds.[b] <- AABBProc.UnionWithAABB(bucketBounds.[b], aabbOfIndex)

                // Using scans O(N)
                let foldCount countAcc count = count + countAcc
                let foldBounds boundsAcc bounds = AABBProc.UnionWithAABB(boundsAcc, bounds)

                let countScanFwd = Array.scan foldCount 0 bucketCounts
                let countScanBck = Array.scanBack foldCount bucketCounts 0
                let boundsScanFwd = Array.scan foldBounds (AABB()) bucketBounds
                let boundsScanBck = Array.scanBack foldBounds bucketBounds (AABB())

                for i in 0..nBuckets-2 do
                    let count0 = countScanFwd.[i]
                    let b0 = boundsScanFwd.[i]
                    let count1 = countScanBck.[i]
                    let b1 = boundsScanBck.[i]
                    bucketCosts.[i] <- 
                        traversalCost +
                        ((float32 count0)*AABBProc.SurfaceArea(b0) + (float32 count1)*AABBProc.SurfaceArea(b1)) / (AABBProc.SurfaceArea(centroidBounds))
                

                // Select the bucket with minimal cost
                let struct(minCost, minCostIndex, accIndex) = Array.fold (fun struct(accCost, minIndex, accIndex) v -> if accCost < v then struct(accCost, minIndex, accIndex+1) else struct(v, accIndex+1, accIndex+1)) struct(System.Single.MaxValue, -1, -1) bucketCosts

                //Create leaf or split
                if nPrimitives < minPrimitivesForSplit || minCost < leafCostForSplit nPrimitives then 
                    struct(start, bvhInfoSubArray, [||])
                else
                    let partitionFunc (elem : BVHPrimitive) =
                        let  b = nBuckets_f32 * accessPointBySplitAxis (AABBProc.Offset(centroidBounds, AABBProc.Center(elem.aabb))) dim
                        if b = nBuckets_f32 
                            then nBuckets - 1 <= minCostIndex
                        else 
                            b <= float32 minCostIndex
                    let smaller, largerOrEqual = Array.partition partitionFunc bvhInfoSubArray
                    let mid = start + smaller.Length
                    struct(mid, smaller, largerOrEqual)
        | x -> failwithf "Recursive splitmethod %u not yet implemented" (LanguagePrimitives.EnumToValue x)

    let buildLeafFromMultiplePrimitives bvhInfoArray (geometryArray: 'T []) subArray orderedGeometryList centroidBounds nPrimitives axis =
        let bounds = Array.fold (fun acc (elem : BVHPrimitive) -> AABBProc.UnionWithAABB(acc, elem.aabb)) (AABB()) subArray
        let newOrderedList = Array.fold (fun acc (elem : BVHPrimitive) -> (geometryArray.[elem.indexOfBoundable] :: acc)) orderedGeometryList subArray
        let leaf = Node (BVHBuildNode(SplitAxis.None, orderedGeometryList.Length, nPrimitives, bounds), Empty, Empty)
        (leaf, newOrderedList, 1)

    /// Builds a BST of bounding volumes. Primitives are ordered Smallest-To-Largest along the split axis
    let rec recursiveBuild ( geometryArray : 'T []) (bvhInfoArray : BVHPrimitive []) (start : int) (finish : int) (orderedGeometryList : 'T  list) (splitMethod : SplitMethods) = 
        let nPrimitives = finish - start
        if nPrimitives = 1 then
            let bvhPrimitive = bvhInfoArray.[start]
            let boundableIndex = bvhPrimitive.indexOfBoundable
            let primitive = geometryArray.[boundableIndex]
            let singleItemLeaf = Node (BVHBuildNode(SplitAxis.None, orderedGeometryList.Length, nPrimitives, bvhPrimitive.aabb), Empty, Empty)
            (singleItemLeaf, primitive :: orderedGeometryList, 1)
        else            
            // TODO: investigate Span type for this when upgrading to >= F#4.5
            // TODO: profile this
            let subArray = if nPrimitives = bvhInfoArray.Length then bvhInfoArray else bvhInfoArray.[start..finish-1]
            let centroidBounds = Array.fold (fun acc (elem : BVHPrimitive) -> AABBProc.UnionWithPoint(acc, AABBProc.Center(elem.aabb))) (AABB()) subArray
            let axis = AABBProc.MaximumExtent(centroidBounds)
            // Unusual case e.g. multiple instances of the same geometry
            if accessPointBySplitAxis centroidBounds.PMin axis = accessPointBySplitAxis centroidBounds.PMax axis then
                buildLeafFromMultiplePrimitives bvhInfoArray geometryArray subArray orderedGeometryList centroidBounds nPrimitives axis
            else
                let struct(splitPoint , smallerThanMidArray, largerThanMidArray) = calculateSplitPoint subArray start finish centroidBounds axis splitMethod
                if largerThanMidArray.Length = 0 || smallerThanMidArray.Length = 0 then
                    buildLeafFromMultiplePrimitives bvhInfoArray geometryArray subArray orderedGeometryList centroidBounds nPrimitives axis
                else
                    Array.blit smallerThanMidArray 0 bvhInfoArray start smallerThanMidArray.Length
                    Array.blit largerThanMidArray 0 bvhInfoArray (start + smallerThanMidArray.Length) largerThanMidArray.Length
                    // TODO: investigate Parallel, need to rework sublists as they have a seq dependency
                    let (leftSubTree, leftOrderedSubList, leftTotalNodes) = recursiveBuild geometryArray bvhInfoArray start splitPoint orderedGeometryList splitMethod
                    let (rightSubTree, rightOrderedSubList, rightTotalNodes) = recursiveBuild geometryArray bvhInfoArray splitPoint finish leftOrderedSubList splitMethod
                    let (leftNode, ll, lr)  =  BVHTree.decompose leftSubTree
                    let (rightNode, rl, rr) =  BVHTree.decompose rightSubTree
                    let bvhNode = BVHBuildNode(axis, start, 0, AABBProc.UnionWithAABB(leftNode.aabb, rightNode.aabb))
                    let newTotalNodes = leftTotalNodes + rightTotalNodes + 1
                    (Node (bvhNode, Node (leftNode , ll, lr), Node (rightNode , rl, rr)), rightOrderedSubList, newTotalNodes)

    member this.Build ( geometryArray : 'T []) (splitMethod : SplitMethods) =
        if Array.isEmpty geometryArray then
            (Empty, [||], 0)
        else
            let bvhInfoArray = buildBVHInfoArray geometryArray
            let (bvhTree, orderedGeometryList, totalNodes) = 
                match splitMethod with
                | SplitMethods.SAH -> recursiveBuild geometryArray bvhInfoArray 0 bvhInfoArray.Length [] splitMethod
                | SplitMethods.Middle -> recursiveBuild geometryArray bvhInfoArray 0 bvhInfoArray.Length [] splitMethod
                | SplitMethods.EqualCounts -> recursiveBuild geometryArray bvhInfoArray 0 bvhInfoArray.Length [] splitMethod
                | x -> failwithf "Splitmethod %u not yet implemented" (LanguagePrimitives.EnumToValue x)
            // Need to reverse since implicit indexing via build is for a Queue (FIFO) data structure
            (bvhTree, List.toArray (List.rev orderedGeometryList), totalNodes)


        