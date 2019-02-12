namespace HenzaiFunc.Core.Acceleration


type SplitMethods =
    | SAH = 0 // Surface Area Heuristic
    | HLBVH = 1 // Hierarchical Linear Bounding Volume Hierarchies
    | Middle = 2
    | EqualCounts = 3

type BVHTree<'T when 'T :> Boundable> = 
    | Empty
    | Node of leaf : 'T * left : BVHTree<'T> * right : BVHTree<'T>

 