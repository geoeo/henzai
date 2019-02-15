namespace HenzaiFunc.Core.Types

open System.Runtime.CompilerServices
open System.Runtime.InteropServices

#nowarn "9"
 
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
type BVHBuildNode = 
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
    | Node of node : BVHBuildNode * left : BVHTree * right : BVHTree
    
[<IsReadOnly;Struct>]
type LeafRuntimeNode =
    val primitivesOffset : int
    val nPrimitives : int

    new(primitivesOffsetIn, nPrimitivesIn) = 
        {
            primitivesOffset = primitivesOffsetIn;
            nPrimitives = nPrimitivesIn
        }

/// 8 byte alinged struct
[<IsReadOnly;Struct;StructLayout(LayoutKind.Sequential, Pack=4)>]
type InteriorRuntimeNode =
    val secondChildOffset : int
    val splitAxis : SplitAxis

    new (secondChildOffsetIn, splitAxisIn) =
        {
            secondChildOffset = secondChildOffsetIn;
            splitAxis = splitAxisIn
        }
      
    
/// 32 byte aligned struct for cache efficiency
[<IsReadOnly;Struct;StructLayout(LayoutKind.Explicit)>]
type BVHRuntimeNode =
    [<FieldOffset 8>] val aabb : AABB // 8 bytes on 64 bit
    [<FieldOffset 16>] val interiorNode : InteriorRuntimeNode // 8 bytes
    [<FieldOffset 24>] val leafNode : LeafRuntimeNode // 8 bytes

    new(aabbIn, interiorNodeIn) = {aabb = aabbIn; interiorNode = interiorNodeIn; leafNode = LeafRuntimeNode()}
    new(aabbIn, leafNodeIn) = {aabb = aabbIn; leafNode = leafNodeIn; interiorNode = InteriorRuntimeNode()}
   
