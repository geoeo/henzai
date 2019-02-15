namespace HenzaiFunc.Core.Acceleration

open HenzaiFunc.Core.Types

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
        
            
        
        
        
        
