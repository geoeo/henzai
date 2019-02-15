﻿namespace HenzaiFunc.Core.Acceleration

open HenzaiFunc.Core.Types

// Phyisically Based Rendering Third Edition p. 257
/// Implements methods to generate a BVH BST
module BVHTree =

    let buildBVHInfoArray ( geometryArray : AxisAlignedBoundable []) =
        Array.mapi (fun i (elem : AxisAlignedBoundable) -> BVHPrimitive(i, elem.GetBounds)) geometryArray

    let accessPointBySplitAxis (p : Point) axis = 
        match axis with
        | SplitAxis.X -> p.X
        | SplitAxis.Y -> p.Y
        | SplitAxis.Z -> p.Z
        | x -> failwithf "Accessed point by %u. Should not happen!" (LanguagePrimitives.EnumToValue x)

    let calculateSplitPoint (bvhInfoSubArray : BVHPrimitive []) (start : int) (finish : int) (centroidBounds : AABB) (dim : SplitAxis) (splitMethod : SplitMethods) =
        match splitMethod with
        | SplitMethods.Middle ->
            let midFloat = (accessPointBySplitAxis centroidBounds.PMin dim + accessPointBySplitAxis centroidBounds.PMax dim) / 2.0f
            //TODO: investigate in place solution without allocating
            let smaller, larger = Array.partition (fun (elem : BVHPrimitive) -> accessPointBySplitAxis (AABB.center elem.aabb) dim < midFloat) bvhInfoSubArray
            let mid = start + smaller.Length
            (mid , smaller, larger)
        | x -> failwithf "Recursive splitmethod %u not yet implemented" (LanguagePrimitives.EnumToValue x)

    let decompose tree = 
        match tree with
        | Empty -> failwith "recursiveBuild produced Empty; this can't happen"
        | Node (v, l, r) -> (v, l, r)

    /// Builds a BST of bounding volumes. Primitives are ordered Smallest-To-Largest along the split axis
    let rec recursiveBuild ( geometryArray : AxisAlignedBoundable []) (bvhInfoArray : BVHPrimitive []) (start : int) (finish : int) (orderedGeometryList : AxisAlignedBoundable list) (splitMethod : SplitMethods) = 
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
            let subArray = bvhInfoArray.[start..finish-1]
            let centroidBounds = Array.fold (fun acc (elem : BVHPrimitive) -> AABB.unionWithPoint acc (AABB.center elem.aabb)) (AABB()) subArray
            let axis = AABB.maximumExtent centroidBounds
            // Unusual case e.g. multiple instances of the same geometry
            if accessPointBySplitAxis centroidBounds.PMin axis = accessPointBySplitAxis centroidBounds.PMax axis then
                let bounds = Array.fold (fun acc (elem : BVHPrimitive) -> AABB.unionWithAABB acc elem.aabb) (AABB()) subArray
                let newOrderedList = Array.fold (fun acc (elem : BVHPrimitive) -> (geometryArray.[elem.indexOfBoundable] :: acc)) orderedGeometryList bvhInfoArray
                let leaf = Node (BVHBuildNode(SplitAxis.None, orderedGeometryList.Length, nPrimitives, bounds), Empty, Empty)
                (leaf, newOrderedList, 1)
            else
                let splitPoint , smallerThanMidArray, largerThanMidArray = calculateSplitPoint subArray start finish centroidBounds axis splitMethod
                Array.blit smallerThanMidArray 0 bvhInfoArray start smallerThanMidArray.Length
                Array.blit largerThanMidArray 0 bvhInfoArray (start + smallerThanMidArray.Length) largerThanMidArray.Length
                // TODO: investigate Async
                let (leftSubTree, leftOrderedSubList, leftTotalNodes) = recursiveBuild geometryArray bvhInfoArray start splitPoint orderedGeometryList splitMethod
                let (rightSubTree, rightOrderedSubList, rightTotalNodes) = recursiveBuild geometryArray bvhInfoArray splitPoint finish orderedGeometryList splitMethod
                let leftNode, ll, lr =  decompose leftSubTree
                let rightNode, rl, rr =  decompose rightSubTree
                let bvhNode = BVHBuildNode(axis, start, nPrimitives, AABB.unionWithAABB leftNode.aabb rightNode.aabb)
                let newTotalNodes = leftTotalNodes + rightTotalNodes + 1
                (Node (bvhNode, Node (leftNode , ll, lr), Node (rightNode , rl, rr)), List.concat [leftOrderedSubList; rightOrderedSubList], newTotalNodes)

    let build ( geometryArray : AxisAlignedBoundable []) (splitMethod : SplitMethods) = 
        let bvhInfoArray = buildBVHInfoArray geometryArray
        let (bvhTree, orderedGeometryList, totalNodes) = 
            match splitMethod with
            | SplitMethods.SAH -> recursiveBuild geometryArray bvhInfoArray 0 bvhInfoArray.Length [] splitMethod
            | SplitMethods.Middle -> recursiveBuild geometryArray bvhInfoArray 0 bvhInfoArray.Length [] splitMethod
            | SplitMethods.EqualCounts -> recursiveBuild geometryArray bvhInfoArray 0 bvhInfoArray.Length [] splitMethod
            | x -> failwithf "Splitmethod %u not yet implemented" (LanguagePrimitives.EnumToValue x)
        // We have to reverse here, since list can only append to head
        (bvhTree, List.rev orderedGeometryList, totalNodes)
        