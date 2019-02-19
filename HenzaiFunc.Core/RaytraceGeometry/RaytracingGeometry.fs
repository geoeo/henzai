namespace HenzaiFunc.Core.RaytraceGeometry

open HenzaiFunc.Core.Types
open System.Numerics
open HenzaiFunc.Core

[<AbstractClass>]
type RaytracingGeometry ()  =
    interface Hitable with
    
        // effects shadow acne
        member this.TMin = 0.001f
        member this.TMax = 500.0f
        member this.HasIntersection _ = false
        member this.Intersect _ = (false, 0.0f)
        member this.IntersectionAcceptable _ _ _ _ = false
        member this.NormalForSurfacePoint _ = Vector3.Zero
        member this.IsObstructedBySelf _ = false

    interface AxisAlignedBoundable with
        member this.GetBounds = AABB(Vector3.Zero, Vector3.Zero)
        member this.IsBoundable = false
   
    member this.AsBoundable = this :> AxisAlignedBoundable
    member this.AsHitable = this :> Hitable

