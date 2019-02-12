namespace HenzaiFunc.Core.Acceleration

open System

type SplitMethods =
    | SAH = 0 // Surface Area Heuristic
    | HLBVH = 1 // Hierarchical Linear Bounding Volume Hierarchies
    | Middle = 2
    | EqualCounts = 3

[<Struct>]
type BVHPrimitive = 
    // the index of the geometry in its array
    val indexOfBoundable : int
    val aabb : AABB

type BVHTree = 
    | Empty
    | Node of leaf : BVHPrimitive * left : BVHTree * right : BVHTree

module BVHTree =

    let recursiveBuild (primitiveList : BVHPrimitive [])
        = (Empty, 0, [|0|])

    let build (primitiveList : BVHPrimitive []) (splitMethod : SplitMethods) = 
        match splitMethod with
        | SplitMethods.SAH -> recursiveBuild primitiveList
        | x -> failwithf "Splitmethod %i not yet implemented" (LanguagePrimitives.EnumToValue x)
        