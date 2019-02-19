namespace HenzaiFunc.Core.RaytraceGeometry

open System.Numerics
open HenzaiFunc.Core.Types


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

    interface Hitable with

        // effects shadow acne
        member this.TMin = 0.001f
        member this.TMax = 500.0f
        member this.HasIntersection _ = false
        member this.Intersect _ = (false, 0.0f)
        member this.IntersectionAcceptable _ _ _ _ = false
        member this.NormalForSurfacePoint _ = Vector3.Zero
        member this.IsObstructedBySelf _ = false

    new() = AABB(Vector3(System.Single.MaxValue), Vector3(System.Single.MinValue))