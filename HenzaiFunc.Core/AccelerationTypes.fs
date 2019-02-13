namespace HenzaiFunc.Core.Types

open System.Numerics
 
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

type AABB(pMin : MinPoint, pMax : MaxPoint) = 

    let boundingCorners : Point[] = [|pMin; pMax|]

    member this.Corner (index : int) =

        let cornerXIndex = index &&& 1
        let cornerYIndex = index &&& 2
        let cornerZIndex = index &&& 4

        let cornerX = boundingCorners.[cornerXIndex].X
        let cornerY = boundingCorners.[cornerYIndex].Y
        let cornerZ = boundingCorners.[cornerZIndex].Z

        Vector3(cornerX, cornerY, cornerZ) : Point

    member this.PMin = boundingCorners.[0]
    member this.PMax = boundingCorners.[1]

    new() = AABB(Vector3(System.Single.MaxValue), Vector3(System.Single.MinValue))

type AxisAlignedBoundable =
    abstract member GetBounds: AABB
    abstract member IsBoundable: bool

/// A layer of abstraction to index a geometry in the main array/storage
[<Struct>]
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
[<Struct>]
type BVHNode = 
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
    | Node of leaf : BVHNode * left : BVHTree * right : BVHTree