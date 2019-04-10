﻿namespace HenzaiFunc.Core.RaytraceGeometry

open System.Numerics
open HenzaiFunc.Core
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

        Vector4(cornerX, cornerY, cornerZ, 0.0f) : Point

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
            let invDir = Vector4(1.0f / ray.Direction.X, 1.0f/ ray.Direction.Y, 1.0f / ray.Direction.Z, 0.0f)
            let (isXDirNeg, isYDirNeg, isZDirNeg) = (invDir.X < 0.0f, invDir.Y < 0.0f, invDir.Z < 0.0f)
            let gamma3 = (RaytraceGeometryUtils.gamma 3)

            let mutable tMin = (this.boundingCorners.[Utils.boolToInt isXDirNeg].X - ray.Origin.X) * invDir.X
            let mutable tMax = (this.boundingCorners.[1 - Utils.boolToInt isXDirNeg].X - ray.Origin.X) * invDir.X
            let tyMin = (this.boundingCorners.[Utils.boolToInt isYDirNeg].Y - ray.Origin.Y) * invDir.Y
            let mutable tyMax = (this.boundingCorners.[1 - Utils.boolToInt isYDirNeg].Y - ray.Origin.Y) * invDir.Y
            let tzMin = (this.boundingCorners.[Utils.boolToInt isZDirNeg].Z - ray.Origin.Z) * invDir.Z
            let mutable tzMax = (this.boundingCorners.[1 - Utils.boolToInt isZDirNeg].Z - ray.Origin.Z) * invDir.Z
            tMax <- RaytraceGeometryUtils.robustRayBounds tMax gamma3
            tyMax <- RaytraceGeometryUtils.robustRayBounds tyMax gamma3
            tzMax <- RaytraceGeometryUtils.robustRayBounds tzMax gamma3
            let passed = not (tMin > tyMax || tyMin > tMax || tMin > tzMax || tzMin > tMax)

            if tyMin > tMin then tMin <- tyMin else ()
            if tyMax < tMax then tMax <- tyMax else ()
            if tzMin > tMin then tMin <- tzMin else ()
            if tzMax < tMax then tMax <- tzMax else ()

            struct(tMin < this.AsHitable.TMax && tMax > 0.0f && passed , tMin)

        member this.HasIntersection ray =
            let struct(hasIntersection, t) = this.AsHitable.Intersect ray 
            let p = ray.Origin + t*ray.Direction
            this.AsHitable.IntersectionAcceptable hasIntersection t 0.0f p
        member this.IntersectionAcceptable hasIntersection t _ _ = hasIntersection
        member this.NormalForSurfacePoint _ = Vector4.Zero
        member this.IsObstructedBySelf _ = false

    new() = AABB(Vector4(System.Single.MaxValue), Vector4(System.Single.MinValue))
    new(pMin : Vector3, pMax: Vector3) = AABB(Vector4(pMin,1.0f), Vector4(pMax, 1.0f))

