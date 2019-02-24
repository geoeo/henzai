namespace HenzaiFunc.Core.RaytraceGeometry

open System
open System.Numerics
open HenzaiFunc.Core.Types


type AABB(pMin : MinPoint, pMax : MaxPoint) = 

    member this.boundingCorners : Point[] = [|pMin; pMax|]

    member this.Corner (index : int) =

        let cornerXIndex = index &&& 1
        let cornerYIndex = index &&& 2
        let cornerZIndex = index &&& 4

        let cornerX = this.boundingCorners.[cornerXIndex].X
        let cornerY = this.boundingCorners.[cornerYIndex].Y
        let cornerZ = this.boundingCorners.[cornerZIndex].Z

        Vector3(cornerX, cornerY, cornerZ) : Point

    member this.PMin = this.boundingCorners.[0]
    member this.PMax = this.boundingCorners.[1]
    member this.AsHitable = this :> Hitable

    interface Hitable with

        // effects shadow acne
        member this.TMin = 0.001f
        member this.TMax = 500.0f
        // Optimized Ray Box Intersection Phyisically Based Rendering Third Edition p. 129
        // TODO: investigate performance improvement when passing in invDir
        member this.Intersect ray = 
            //let mutable isAcceptable = true
            let invDir = Vector3(1.0f / ray.Direction.X, 1.0f/ ray.Direction.Y, 1.0f / ray.Direction.Z)
            let struct(isXDirNeg, isYDirNeg, isZDirNeg) = struct(invDir.X < 0.0f, invDir.Y < 0.0f, invDir.Z < 0.0f)
            let gamma3 = (RaytraceGeometryUtils.gamma 3)

            let mutable tMin = (this.boundingCorners.[RaytraceGeometryUtils.boolToInt isXDirNeg].X - ray.Origin.X) * invDir.X
            let mutable tMax = (this.boundingCorners.[1 - RaytraceGeometryUtils.boolToInt isXDirNeg].X - ray.Origin.X) * invDir.X
            let tyMin = (this.boundingCorners.[RaytraceGeometryUtils.boolToInt isYDirNeg].Y - ray.Origin.Y) * invDir.Y
            let mutable tyMax = (this.boundingCorners.[1 - RaytraceGeometryUtils.boolToInt isYDirNeg].Y - ray.Origin.Y) * invDir.Y
            let tzMin = (this.boundingCorners.[RaytraceGeometryUtils.boolToInt isZDirNeg].Z - ray.Origin.Z) * invDir.Z
            let mutable tzMax = (this.boundingCorners.[1 - RaytraceGeometryUtils.boolToInt isZDirNeg].Z - ray.Origin.Z) * invDir.Z
            tMax <- RaytraceGeometryUtils.robustRayBounds tMax gamma3
            tyMax <- RaytraceGeometryUtils.robustRayBounds tyMax gamma3
            tzMax <- RaytraceGeometryUtils.robustRayBounds tzMax gamma3
            let passed = not (tMin > tyMax || tyMin > tMax || tMin > tzMax || tzMin > tMax)

            if tyMin > tMin then tMin <- tyMin else ()
            if tyMax < tMax then tMax <- tyMax else ()
            if tzMin > tMin then tMin <- tzMin else ()
            if tzMax < tMax then tMax <- tzMax else ()

            // if insinde a box tMin might be negative 
            // in that case return tMax
            if tMin < 0.0f then tMin <- tMax else ()


            (tMin > this.AsHitable.TMin && tMin < this.AsHitable.TMax && tMax > 0.0f && passed , tMin)

        member this.HasIntersection ray =
            let (hasIntersection, t) = this.AsHitable.Intersect ray 
            let p = ray.Origin + t*ray.Direction
            this.AsHitable.IntersectionAcceptable hasIntersection t 0.0f p
        member this.IntersectionAcceptable hasIntersection t _ _ = hasIntersection && t > this.AsHitable.TMin
        member this.NormalForSurfacePoint _ = Vector3.Zero
        member this.IsObstructedBySelf _ = false

    new() = AABB(Vector3(System.Single.MaxValue), Vector3(System.Single.MinValue))

