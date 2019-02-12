namespace HenzaiFunc.Core.Acceleration

type SplitMethods =
    | SAH = 0us // Surface Area Heuristic
    | HLBVH = 1us // Hierarchical Linear Bounding Volume Hierarchies
    | Middle = 2us
    | EqualCounts = 3us

type SplitAxis = 
    | X = 0us
    | Y = 1us
    | Z = 2us
    | None = 4us

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

module BVHTree =

    let recursiveBuild (primitiveArray : BVHPrimitive []) (start : int) (finish : int) = 
        let nPrimitives = finish - start

        if nPrimitives = 1 then
            let leaf = BVHNode(SplitAxis.None, start, nPrimitives, primitiveArray.[start].aabb)
            let tree = Node (leaf, Empty, Empty)
            (tree , nPrimitives, [primitiveArray.[start]])
        else
            //TODO: implement recursive case
            let tree = Empty
            let orderedPrimitives = [primitiveArray.[start]]
            (tree, nPrimitives , orderedPrimitives)

    let build (primitiveArray : BVHPrimitive []) (splitMethod : SplitMethods) = 
        match splitMethod with
        | SplitMethods.SAH -> recursiveBuild primitiveArray 0 primitiveArray.Length
        | x -> failwithf "Splitmethod %u not yet implemented" (LanguagePrimitives.EnumToValue x)
        