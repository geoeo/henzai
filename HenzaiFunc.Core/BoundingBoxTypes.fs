namespace HenzaiFunc.Core.Types

open System.Numerics

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