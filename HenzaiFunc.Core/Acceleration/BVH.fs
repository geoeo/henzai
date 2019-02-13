namespace HenzaiFunc.Core.Acceleration

open HenzaiFunc.Core.Types

module BVHTree =

    let recursiveBuild (primitiveArray : BVHPrimitive []) (start : int) (finish : int) = 
        let nPrimitives = finish - start

        if nPrimitives = 1 then
            let leaf = BVHNode(SplitAxis.None, start, nPrimitives, primitiveArray.[start].aabb)
            let tree = Node (leaf, Empty, Empty)
            (tree , nPrimitives, [primitiveArray.[start]])
        else
            //TODO: implement recursive case
            // compute bounds of centroid
            // if bounds has no volume -> leaf (unusual)
            // else partition based on split
            let tree = Empty
            let orderedPrimitives = [primitiveArray.[start]]
            (tree, nPrimitives , orderedPrimitives)

    let build (primitiveArray : BVHPrimitive []) (splitMethod : SplitMethods) = 
        match splitMethod with
        | SplitMethods.SAH -> recursiveBuild primitiveArray 0 primitiveArray.Length
        | x -> failwithf "Splitmethod %u not yet implemented" (LanguagePrimitives.EnumToValue x)
        