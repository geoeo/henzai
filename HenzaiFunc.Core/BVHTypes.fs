namespace HenzaiFunc.Core.Types

open System.Runtime.CompilerServices
 
type SplitMethods =
    | SAH = 0uy // Surface Area Heuristic
    | HLBVH = 1uy // Hierarchical Linear Bounding Volume Hierarchies
    | Middle = 2uy
    | EqualCounts = 3uy

type SplitAxis = 
    | X = 0uy
    | Y = 1uy
    | Z = 2uy
    | None = 4uy
   

/// A layer of abstraction to index a geometry in the main array/storage
[<IsReadOnly;Struct>]
type BVHPrimitive = 
    // the index of the geometry in its array (primitiveNumber)
    val indexOfBoundable : int
    val aabb : AABB

    new (indexOfBoundableIn, aabbIn) = 
        {
            indexOfBoundable = indexOfBoundableIn;
            aabb = aabbIn
        }

/// A node of the BVH Tree
/// Not used for runtime traversal
[<IsReadOnly;Struct>]
type BVHNodeBVHBuildNode = 
    val splitAxis : SplitAxis
    // the starting location of contained primtives in the primitive array
    val firstPrimitiveOffset : int
    // the number of primitives contained in the AABB
    val nPrimitives : int
    val aabb : AABB

    new(splitAxisIn, firstPrimitiveOffsetIn, nPrimitivesIn, aabbIn) =
        {
            splitAxis = splitAxisIn;
            firstPrimitiveOffset = firstPrimitiveOffsetIn;
            nPrimitives = nPrimitivesIn;
            aabb = aabbIn
        }

type BVHTree = 
    | Empty
    | Node of leaf : BVHNodeBVHBuildNode * left : BVHTree * right : BVHTree