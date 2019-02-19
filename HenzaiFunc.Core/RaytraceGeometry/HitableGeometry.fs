namespace HenzaiFunc.Core.RaytraceGeometry

open HenzaiFunc.Core.Types
open System.Numerics
open HenzaiFunc.Core


interface IHitable with

    abstract member HasIntersection: Ray -> bool
    abstract member Intersect: Ray -> bool*LineParameter
    abstract member IntersectionAcceptable : bool -> LineParameter -> float32 -> Point -> bool
    abstract member NormalForSurfacePoint : Point -> Normal
    abstract member IsObstructedBySelf: Ray -> bool 
    abstract member TMin : float32
    abstract member TMax : float32

[<AbstractClass>]
type Hitable ()  =
    interface IHitable with
    
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

