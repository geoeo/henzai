﻿namespace HenzaiFunc.Core.Acceleration

open HenzaiFunc.Core.Types
open System

// Phyisically Based Rendering Third Edition p. 257

module BVHTree =

    let accessPointBySplitDim (p : Point) dim = 
        match dim with
        | SplitAxis.X -> p.X
        | SplitAxis.Y -> p.Y
        | SplitAxis.Z -> p.Z
        | x -> failwithf "Accessed point by %u. Should not happen!" (LanguagePrimitives.EnumToValue x)

    let calculateSplitPoint (primitiveSubArray : BVHPrimitive []) (start : int) (finish : int) (centroidBounds : AABB) (dim : SplitAxis) (splitMethod : SplitMethods) =
        match splitMethod with
        | SplitMethods.Middle ->
            let midFloat = accessPointBySplitDim centroidBounds.PMin dim + accessPointBySplitDim centroidBounds.PMax dim / 2.0f
            let smaller, larger = Array.partition (fun (elem : BVHPrimitive) -> accessPointBySplitDim (AABB.center elem.aabb) dim < midFloat) primitiveSubArray
            let mid = smaller.Length
            (mid , [])
        | x -> failwithf "Recursive splitmethod %u not yet implemented" (LanguagePrimitives.EnumToValue x)

    let rec recursiveBuild ( geometryArray : AxisAlignedBoundable []) (primitiveArray : BVHPrimitive []) (start : int) (finish : int) (orderedGeometryList : AxisAlignedBoundable list) (splitMethod : SplitMethods) = 
        let nPrimitives = finish - start
        if nPrimitives = 1 then
            let bvhPrimitive = primitiveArray.[start]
            let boundableIndex = bvhPrimitive.indexOfBoundable
            let primitive = geometryArray.[boundableIndex]
            let singleItemLeaf = Node (BVHNode(SplitAxis.None, orderedGeometryList.Length, nPrimitives, bvhPrimitive.aabb), Empty, Empty)
            (singleItemLeaf, primitive :: orderedGeometryList)
        else            
            // TODO: investigate Span type for this when upgrading to >= F#4.5
            // TODO: profile this
            let subArray = Array.sub primitiveArray start nPrimitives
            let centroidBounds = Array.fold (fun acc (elem : BVHPrimitive) -> AABB.unionWithPoint acc (AABB.center elem.aabb)) (AABB()) subArray
            let dim = AABB.maximumExtent centroidBounds
            // Very rare case
            if accessPointBySplitDim centroidBounds.PMin dim = accessPointBySplitDim centroidBounds.PMax dim then
                let bounds = Array.fold (fun acc (elem : BVHPrimitive) -> AABB.unionWithAABB acc elem.aabb) (AABB()) subArray
                let newOrderedList = Array.fold (fun acc (elem : BVHPrimitive) -> (geometryArray.[elem.indexOfBoundable] :: acc)) orderedGeometryList primitiveArray
                let leaf = Node (BVHNode(SplitAxis.None, orderedGeometryList.Length, nPrimitives, bounds), Empty, Empty)
                (leaf, newOrderedList)
            else
                let splitPoint , orderedGeometryList = calculateSplitPoint subArray start finish centroidBounds dim splitMethod
                // TODO: investigate Async
                let (Node (leftNode, ll, lr), leftSubList) = recursiveBuild geometryArray primitiveArray start splitPoint orderedGeometryList splitMethod
                let (Node (rightNode,rl, rr), rightSubList) = recursiveBuild geometryArray primitiveArray splitPoint finish orderedGeometryList splitMethod
                let bvhNode = BVHNode(dim, start, nPrimitives, AABB.unionWithAABB leftNode.aabb rightNode.aabb)
                (Node (bvhNode, Node (leftNode , ll, lr), Node (rightNode , rl, rr)), List.concat [leftSubList; rightSubList])

    let build ( geometryArray : AxisAlignedBoundable []) (primitiveArray : BVHPrimitive []) (splitMethod : SplitMethods) = 
        match splitMethod with
        | SplitMethods.SAH -> recursiveBuild geometryArray primitiveArray 0 primitiveArray.Length [] splitMethod
        | SplitMethods.Middle -> recursiveBuild geometryArray primitiveArray 0 primitiveArray.Length [] splitMethod
        | SplitMethods.EqualCounts -> recursiveBuild geometryArray primitiveArray 0 primitiveArray.Length [] splitMethod
        | x -> failwithf "Splitmethod %u not yet implemented" (LanguagePrimitives.EnumToValue x)
        