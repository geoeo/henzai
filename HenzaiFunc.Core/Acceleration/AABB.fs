module HenzaiFunc.Core.Acceleration.AABB

open System.Numerics
open HenzaiFunc.Core.Types

type AABB(pMin : MinPoint, pMax : MaxPoint) = 

    let boundingCorners : Point[] = [|pMin; pMax|]

    let corner (index : int) =
    
       let cornerXIndex = index &&& 1
       let cornerYIndex = index &&& 2
       let cornerZIndex = index &&& 4

       let cornerX = boundingCorners.[cornerXIndex].X
       let cornerY = boundingCorners.[cornerYIndex].Y
       let cornerZ = boundingCorners.[cornerZIndex].Z

       Vector3(cornerX, cornerY, cornerZ) : Point

    //TODO:

    let union = ()

    let intersect = ()

    let overlaps = ()

    let inside = ()

    let insideExclusive = ()

    let expand = ()

    let diagonal = ()

    let surfaceArea = ()

    let volume = ()

    let maximumExtent = ()

    let lerp = ()

    let offset = ()

    let boundingSphere = ()







