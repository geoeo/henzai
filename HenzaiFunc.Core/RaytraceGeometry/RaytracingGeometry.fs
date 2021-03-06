namespace HenzaiFunc.Core.RaytraceGeometry

open System.Numerics
open Henzai.Core.Acceleration
open Henzai.Core.Raytracing

[<AbstractClass>]
type RaytracingGeometry ()  =
    interface Hitable with
    
        // effects shadow acne
        member this.TMin() = 0.001f
        member this.TMax() = 500.0f
        member this.HasIntersection _ = false
        member this.Intersect _ = struct(false, 0.0f)
        member this.IntersectionAcceptable(_, _, _, _) = false
        member this.NormalForSurfacePoint _ = Vector4.Zero
        member this.IsObstructedBySelf _ = false

    interface AxisAlignedBoundable with
        member this.GetBounds() = AABB(Vector4.Zero, Vector4.Zero)
        member this.IsBoundable() = false
   
    member this.AsBoundable = this :> AxisAlignedBoundable
    member this.AsHitable = this :> Hitable

