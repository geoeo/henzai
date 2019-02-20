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
    member this.AsHitable = this :> Hitable

    interface Hitable with

        // effects shadow acne
        member this.TMin = 0.001f
        member this.TMax = 500.0f
        member this.Intersect ray = 
            let mutable t0 = this.AsHitable.TMin
            let mutable t1 = this.AsHitable.TMax
            let mutable isAcceptable = true
            let gamma3 = (RaytraceGeometryUtils.gamma 3)
            for i in 0..2 do
                match i with
                | 0 ->
                    let invRayDir = 1.0f / ray.Direction.X
                    let tNear1 = (this.PMin.X - ray.Origin.X) * invRayDir
                    let tFar1 = (this.PMax.X - ray.Origin.X) * invRayDir
                    let (tNear2, tFar2) = RaytraceGeometryUtils.conditionalSwap ( > ) tNear1 tFar1
                    let tFar3 = RaytraceGeometryUtils.robustRayBounds tFar2 gamma3
                    t0 <- if tNear2 > t0 then tNear2 else t0
                    t1 <- if tFar3 < t1 then tFar3 else t1
                    ()
                | 1 ->
                    let invRayDir = 1.0f / ray.Direction.Y
                    let tNear1 = (this.PMin.Y - ray.Origin.Y) * invRayDir
                    let tFar1 = (this.PMax.Y - ray.Origin.Y) * invRayDir
                    let (tNear2, tFar2) = RaytraceGeometryUtils.conditionalSwap ( > ) tNear1 tFar1
                    let tFar3 = RaytraceGeometryUtils.robustRayBounds tFar2 gamma3
                    t0 <- if tNear2 > t0 then tNear2 else t0
                    t1 <- if tFar3 < t1 then tFar3 else t1
                    ()
                | 2 ->
                    let invRayDir = 1.0f / ray.Direction.Z
                    let tNear1 = (this.PMin.Z - ray.Origin.Z) * invRayDir
                    let tFar1 = (this.PMax.Z - ray.Origin.Z) * invRayDir
                    let (tNear2, tFar2) = RaytraceGeometryUtils.conditionalSwap ( > ) tNear1 tFar1
                    let tFar3 = RaytraceGeometryUtils.robustRayBounds tFar2 gamma3
                    t0 <- if tNear2 > t0 then tNear2 else t0
                    t1 <- if tFar3 < t1 then tFar3 else t1
                    ()
                | x -> failwithf "value %u not possible in AABB interection" x
                isAcceptable <- t1 > t0
            (isAcceptable, t0)
            
        member this.HasIntersection ray =
            let (hasIntersection,_) = this.AsHitable.Intersect ray 
            hasIntersection
        member this.IntersectionAcceptable hasIntersection _ _ _ = hasIntersection
        member this.NormalForSurfacePoint _ = Vector3.Zero
        member this.IsObstructedBySelf _ = false

    new() = AABB(Vector3(System.Single.MaxValue), Vector3(System.Single.MinValue))

